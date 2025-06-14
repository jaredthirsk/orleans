using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Configuration;
using Forkleans.Runtime;
using Forkleans.Timers.Internal;
using Forkleans.Transactions.Abstractions;
using Forkleans.Transactions.State;
using Forkleans.Transactions.TOC;

namespace Forkleans.Transactions
{
    public partial class TransactionCommitter<TService> : ITransactionCommitter<TService>, ILifecycleParticipant<IGrainLifecycle>
        where TService : class
    {
        private readonly ITransactionCommitterConfiguration config;
        private readonly IGrainContext context;
        private readonly ITransactionDataCopier<OperationState> copier;
        private readonly IGrainRuntime grainRuntime;
        private readonly ActivationLifetime activationLifetime;
        private readonly ILogger logger;
        private ParticipantId participantId;
        private TransactionQueue<OperationState> queue;

        private bool detectReentrancy;

        public TransactionCommitter(
            ITransactionCommitterConfiguration config,
            IGrainContextAccessor contextAccessor,
            ITransactionDataCopier<OperationState> copier,
            IGrainRuntime grainRuntime,
            ILogger<TransactionCommitter<TService>> logger)
        {
            this.config = config;
            this.context = contextAccessor.GrainContext;
            this.copier = copier;
            this.grainRuntime = grainRuntime;
            this.logger = logger;
            this.activationLifetime = new ActivationLifetime(this.context);
        }

        /// <inheritdoc/>
        public Task OnCommit(ITransactionCommitOperation<TService> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (detectReentrancy)
            {
                throw new LockRecursionException("cannot perform an update operation from within another operation");
            }

            var info = TransactionContext.GetRequiredTransactionInfo();

            LogTraceStartWrite(info);
            if (info.IsReadOnly)
            {
                throw new OrleansReadOnlyViolatedException(info.Id);
            }

            info.Participants.TryGetValue(this.participantId, out var recordedaccesses);

            return this.queue.RWLock.EnterLock<bool>(info.TransactionId, info.Priority, recordedaccesses, false,
                () =>
                {
                    // check if we expired while waiting
                    if (!this.queue.RWLock.TryGetRecord(info.TransactionId, out TransactionRecord<OperationState> record))
                    {
                        throw new OrleansCascadingAbortException(info.TransactionId.ToString());
                    }

                    // merge the current clock into the transaction time stamp
                    record.Timestamp = this.queue.Clock.MergeUtcNow(info.TimeStamp);

                    // link to the latest state
                    if (record.State == null)
                    {
                        this.queue.GetMostRecentState(out record.State, out record.SequenceNumber);
                    }

                    // if this is the first write, make a deep copy of the state
                    if (!record.HasCopiedState)
                    {
                        record.State = this.copier.DeepCopy(record.State);
                        record.SequenceNumber++;
                        record.HasCopiedState = true;
                    }

                    LogDebugUpdateLockWrite(record.SequenceNumber, record.TransactionId, new(record.Timestamp));

                    // record this write in the transaction info data structure
                    info.RecordWrite(this.participantId, record.Timestamp);

                    // perform the write
                    try
                    {
                        detectReentrancy = true;

                        record.State.Operation = operation;
                        return true;
                    }
                    finally
                    {
                        LogTraceEndWrite(info, record.TransactionId, new(record.Timestamp));
                        detectReentrancy = false;
                    }
                }
            );
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe<TransactionalState<OperationState>>(GrainLifecycleStage.SetupState, OnSetupState);
        }

        private async Task OnSetupState(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            this.participantId = new ParticipantId(this.config.ServiceName, this.context.GrainReference, ParticipantId.Role.Resource | ParticipantId.Role.PriorityManager);

            var storageFactory = this.context.ActivationServices.GetRequiredService<INamedTransactionalStateStorageFactory>();
            ITransactionalStateStorage<OperationState> storage = storageFactory.Create<OperationState>(this.config.StorageName, this.config.ServiceName);

            // setup transaction processing pipe
            void deactivate() => grainRuntime.DeactivateOnIdle(context);
            var options = this.context.ActivationServices.GetRequiredService<IOptions<TransactionalStateOptions>>();
            var clock = this.context.ActivationServices.GetRequiredService<IClock>();
            TService service = this.context.ActivationServices.GetRequiredKeyedService<TService>(this.config.ServiceName);
            var timerManager = this.context.ActivationServices.GetRequiredService<ITimerManager>();
            this.queue = new TocTransactionQueue<TService>(service, options, this.participantId, deactivate, storage, clock, logger, timerManager, this.activationLifetime);

            // Add transaction manager factory to the grain context
            this.context.RegisterResourceFactory<ITransactionManager>(this.config.ServiceName, () => new TransactionManager<OperationState>(this.queue));

            // recover state
            await this.queue.NotifyOfRestore();
        }

        [Serializable]
        [GenerateSerializer]
        public sealed class OperationState
        {
            [Id(0)]
            public ITransactionCommitOperation<TService> Operation { get; set; }
        }

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "StartWrite {Info}"
        )]
        private partial void LogTraceStartWrite(TransactionInfo info);

        private readonly struct DateTimeLogRecord(DateTime ts)
        {
            public override string ToString() => ts.ToString("o");
        }

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Update-lock write v{SequenceNumber} {TransactionId} {Timestamp}"
        )]
        private partial void LogDebugUpdateLockWrite(long sequenceNumber, Guid transactionId, DateTimeLogRecord timestamp);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "EndWrite {Info} {TransactionId} {Timestamp}"
        )]
        private partial void LogTraceEndWrite(TransactionInfo info, Guid transactionId, DateTimeLogRecord timestamp);
    }
}
