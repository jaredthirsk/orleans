using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Forkleans.Core;
using Forkleans.Runtime;
using Forkleans.Storage;
using Forkleans.Transactions.Abstractions;

#nullable enable
namespace Forkleans.Transactions
{
    internal sealed class TransactionalStateStorageProviderWrapper<TState> : ITransactionalStateStorage<TState>
        where TState : class, new()
    {
        private readonly IGrainStorage grainStorage;
        private readonly IGrainContext context;
        private readonly string stateName;

        private StateStorageBridge<TransactionalStateRecord<TState>>? stateStorage;
        [MemberNotNull(nameof(stateStorage))]
        private StateStorageBridge<TransactionalStateRecord<TState>> StateStorage => stateStorage ??= GetStateStorage();

        public TransactionalStateStorageProviderWrapper(IGrainStorage grainStorage, string stateName, IGrainContext context)
        {
            this.grainStorage = grainStorage;
            this.context = context;
            this.stateName = stateName;
        }

        public async Task<TransactionalStorageLoadResponse<TState>> Load()
        {
            await this.StateStorage.ReadStateAsync();
            var state = stateStorage.State;
            return new TransactionalStorageLoadResponse<TState>(stateStorage.Etag, state.CommittedState, state.CommittedSequenceId, state.Metadata, state.PendingStates);
        }

        public async Task<string> Store(string expectedETag, TransactionalStateMetaData metadata, List<PendingTransactionState<TState>> statesToPrepare, long? commitUpTo, long? abortAfter)
        {
            if (this.StateStorage.Etag != expectedETag)
                throw new ArgumentException(nameof(expectedETag), "Etag does not match");
            var state = stateStorage.State;
            state.Metadata = metadata;

            var pendinglist = state.PendingStates;

            // abort
            if (abortAfter.HasValue && pendinglist.Count != 0)
            {
                var pos = pendinglist.FindIndex(t => t.SequenceId > abortAfter.Value);
                if (pos != -1)
                {
                    pendinglist.RemoveRange(pos, pendinglist.Count - pos);
                }
            }

            // prepare
            if (statesToPrepare?.Count > 0)
            {
                foreach (var p in statesToPrepare)
                {
                    var pos = pendinglist.FindIndex(t => t.SequenceId >= p.SequenceId);
                    if (pos == -1)
                    {
                        pendinglist.Add(p); //append
                    }
                    else if (pendinglist[pos].SequenceId == p.SequenceId)
                    {
                        pendinglist[pos] = p;  //replace
                    }
                    else
                    {
                        pendinglist.Insert(pos, p); //insert
                    }
                }
            }

            // commit
            if (commitUpTo.HasValue && commitUpTo.Value > state.CommittedSequenceId)
            {
                var pos = pendinglist.FindIndex(t => t.SequenceId == commitUpTo.Value);
                if (pos != -1)
                {
                    var committedState = pendinglist[pos];
                    state.CommittedSequenceId = committedState.SequenceId;
                    state.CommittedState = committedState.State;
                    pendinglist.RemoveRange(0, pos + 1);
                }
                else
                {
                    throw new InvalidOperationException($"Transactional state corrupted. Missing prepare record (SequenceId={commitUpTo.Value}) for committed transaction.");
                }
            }

            await stateStorage.WriteStateAsync();
            return stateStorage.Etag!;
        }

        private StateStorageBridge<TransactionalStateRecord<TState>> GetStateStorage()
        {
            return new(this.stateName, context, grainStorage);
        }
    }

    [Serializable]
    [GenerateSerializer]
    public sealed class TransactionalStateRecord<TState>
        where TState : class, new()
    {
        [Id(0)]
        public TState CommittedState { get; set; } = new TState();

        [Id(1)]
        public long CommittedSequenceId { get; set; }

        [Id(2)]
        public TransactionalStateMetaData Metadata { get; set; } = new TransactionalStateMetaData();

        [Id(3)]
        public List<PendingTransactionState<TState>> PendingStates { get; set; } = new List<PendingTransactionState<TState>>();
    }
}