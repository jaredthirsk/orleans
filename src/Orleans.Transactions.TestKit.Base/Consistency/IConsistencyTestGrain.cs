using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Forkleans.Transactions.TestKit.Consistency
{
    public interface IConsistencyTestGrain : IGrainWithIntegerKey
    {
        [Transaction(TransactionOption.CreateOrJoin)]
        Task<Observation[]> Run(ConsistencyTestOptions options, int depth, string stack, int max, DateTime stopAfter);
    }


    [Serializable]
    [GenerateSerializer]
    public class UserAbort : Exception
    {
        public UserAbort() : base("User aborted transaction") { }

        [Obsolete]
        protected UserAbort(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

}
