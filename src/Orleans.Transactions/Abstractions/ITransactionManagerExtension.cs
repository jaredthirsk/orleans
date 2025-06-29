using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forkleans.Concurrency;
using Forkleans.Runtime;

namespace Forkleans.Transactions.Abstractions
{
    public interface ITransactionManagerExtension : IGrainExtension
    {
        [AlwaysInterleave]
        [Transaction(TransactionOption.Suppress)]
        Task<TransactionalStatus> PrepareAndCommit(string resourceId, Guid transactionId, AccessCounter accessCount, DateTime timeStamp, List<ParticipantId> writeResources, int totalParticipants);

        [AlwaysInterleave]
        [Transaction(TransactionOption.Suppress)]
        [OneWay]
        Task Prepared(string resourceId, Guid transactionId, DateTime timestamp, ParticipantId resource, TransactionalStatus status);

        [AlwaysInterleave]
        [Transaction(TransactionOption.Suppress)]
        [OneWay]
        Task Ping(string resourceId, Guid transactionId, DateTime timeStamp, ParticipantId resource);
    }
}
