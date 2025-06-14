using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Runtime;
using Forkleans.Transactions.Abstractions;
using Forkleans.Transactions.State;
using Forkleans.Configuration;
using Forkleans.Timers.Internal;

namespace Forkleans.Transactions
{
    /// <summary>
    /// Stateful facet that respects Orleans transaction semantics
    /// </summary>
    public partial class TransactionalState<TState> : ITransactionalState<TState>, ILifecycleParticipant<IGrainLifecycle>
        where TState : class, new()
    {
        private readonly TransactionalStateConfiguration config;
        private readonly IGrainContext context;
        private readonly ITransactionDataCopier<TState> copier;
        private readonly Dictionary<Type,object> copiers;
        private readonly IGrainRuntime grainRuntime;
        private readonly ILogger logger;
        private readonly ActivationLifetime activationLifetime;
        private ParticipantId participantId;
        private TransactionQueue<TState> queue;

        public string CurrentTransactionId => TransactionContext.GetRequiredTransactionInfo().Id;

        private bool detectReentrancy;

        public TransactionalState(
            TransactionalStateConfiguration transactionalStateConfiguration,
            IGrainContextAccessor contextAccessor,
            ITransactionDataCopier<TState> copier,
            IGrainRuntime grainRuntime,
            ILogger<TransactionalState<TState>> logger)
        {
            this.config = transactionalStateConfiguration;
            this.context = contextAccessor.GrainContext;
            this.copier = copier;
            this.grainRuntime = grainRuntime;
            this.logger = logger;
            this.copiers = new Dictionary<Type, object>();
            this.copiers.Add(typeof(TState), copier);
            this.activationLifetime = new ActivationLifetime(this.context);
        }

        /// <summary>
        /// Read the current state.
        /// </summary>
        public Task<TResult> PerformRead<TResult>(Func<TState, TResult> operation)
        {
            if (detectReentrancy)
            {
                throw new LockRecursionException("Cannot perform a read operation from within another operation");
            }

            var info = TransactionContext.GetRequiredTransactionInfo();
            LogTraceStartRead(info);

            info.Participants.TryGetValue(this.participantId, out var recordedaccesses);

            // schedule read access to happen under the lock
            return this.queue.RWLock.EnterLock<TResult>(info.TransactionId, info.Priority, recordedaccesses, true,
                () =>
                {
                    // check if our record is gone because we expired while waiting
                    if (!this.queue.RWLock.TryGetRecord(info.TransactionId, out TransactionRecord<TState> record))
                    {
                        throw new OrleansCascadingAbortException(info.TransactionId.ToString());
                    }

                    // merge the current clock into the transaction time stamp
                    record.Timestamp = this.queue.Clock.MergeUtcNow(info.TimeStamp);

                    if (record.State == null)
                    {
                        this.queue.GetMostRecentState(out record.State, out record.SequenceNumber);
                    }

                    LogDebugUpdateLockRead(record.SequenceNumber, record.TransactionId, new(record.Timestamp));

                    // record this read in the transaction info data structure
                    info.RecordRead(this.participantId, record.Timestamp);

                    // perform the read
                    TResult result = default;
                    try
                    {
                        detectReentrancy = true;

                        result = CopyResult(operation(record.State));
                    }
                    finally
                    {
                        LogTraceEndRead(info, result, record.State);
                        detectReentrancy = false;
                    }

                    return result;
                });
        }

        /// <inheritdoc/>
        public Task<TResult> PerformUpdate<TResult>(Func<TState, TResult> updateAction)
        {
            if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));
            if (detectReentrancy)
            {
                throw new LockRecursionException("Cannot perform an update operation from within another operation");
            }

            var info = TransactionContext.GetRequiredTransactionInfo();

            LogTraceStartWrite(info);

            if (info.IsReadOnly)
            {
                throw new OrleansReadOnlyViolatedException(info.Id);
            }

            info.Participants.TryGetValue(this.participantId, out var recordedaccesses);

            return this.queue.RWLock.EnterLock<TResult>(info.TransactionId, info.Priority, recordedaccesses, false,
                () =>
                {
                    // check if we expired while waiting
                    if (!this.queue.RWLock.TryGetRecord(info.TransactionId, out TransactionRecord<TState> record))
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

                        return CopyResult(updateAction(record.State));
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
            lifecycle.Subscribe<TransactionalState<TState>>(GrainLifecycleStage.SetupState, (ct) => OnSetupState(SetupResourceFactory, ct));
        }

        private static void SetupResourceFactory(IGrainContext context, string stateName, TransactionQueue<TState> queue)
        {
            // Add resources factory to the grain context
            context.RegisterResourceFactory<ITransactionalResource>(stateName, () => new TransactionalResource<TState>(queue));

            // Add tm factory to the grain context
            context.RegisterResourceFactory<ITransactionManager>(stateName, () => new TransactionManager<TState>(queue));
        }

        internal async Task OnSetupState(Action<IGrainContext, string, TransactionQueue<TState>> setupResourceFactory, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            this.participantId = new ParticipantId(this.config.StateName, this.context.GrainReference, this.config.SupportedRoles);

            var storageFactory = this.context.ActivationServices.GetRequiredService<INamedTransactionalStateStorageFactory>();
            ITransactionalStateStorage<TState> storage = storageFactory.Create<TState>(this.config.StorageName, this.config.StateName);

            // setup transaction processing pipe
            void deactivate() => grainRuntime.DeactivateOnIdle(context);
            var options = this.context.ActivationServices.GetRequiredService<IOptions<TransactionalStateOptions>>();
            var clock = this.context.ActivationServices.GetRequiredService<IClock>();
            var timerManager = this.context.ActivationServices.GetRequiredService<ITimerManager>();
            this.queue = new TransactionQueue<TState>(options, this.participantId, deactivate, storage, clock, logger, timerManager, this.activationLifetime);

            setupResourceFactory(this.context, this.config.StateName, queue);

            // recover state
            await this.queue.NotifyOfRestore();
        }

        private TResult CopyResult<TResult>(TResult result)
        {
            ITransactionDataCopier<TResult> resultCopier;
            if (!this.copiers.TryGetValue(typeof(TResult), out object cp))
            {
                resultCopier = this.context.ActivationServices.GetRequiredService<ITransactionDataCopier<TResult>>();
                this.copiers.Add(typeof(TResult), resultCopier);
            }
            else
            {
                resultCopier = (ITransactionDataCopier<TResult>)cp;
            }
            return resultCopier.DeepCopy(result);
        }

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "StartRead {Info}"
        )]
        private partial void LogTraceStartRead(TransactionInfo info);

        private readonly struct DateTimeLogRecord(DateTime ts)
        {
            public override string ToString() => ts.ToString("o");
        }

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Update-lock read v{SequenceNumber} {TransactionId} {Timestamp}"
        )]
        private partial void LogDebugUpdateLockRead(long sequenceNumber, Guid transactionId, DateTimeLogRecord timestamp);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "EndRead {Info} {Result} {State}"
        )]
        private partial void LogTraceEndRead(TransactionInfo info, object result, TState state);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "StartWrite {Info}"
        )]
        private partial void LogTraceStartWrite(TransactionInfo info);

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
