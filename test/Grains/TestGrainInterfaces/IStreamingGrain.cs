using Microsoft.Extensions.Logging;
using Forkleans.Streams;

namespace UnitTests.GrainInterfaces
{
    public interface IStreaming_ConsumerGrain : IGrainWithGuidKey
    {
        Task BecomeConsumer(Guid streamId, string providerToUse, string streamNamespace);
        Task StopBeingConsumer();
        Task<int> GetItemsConsumed();
        Task<int> GetConsumerCount();
        Task DeactivateConsumerOnIdle();
    }

    public interface IPersistentStreaming_ProducerGrain : IStreaming_ProducerGrain
    {
    }

    public interface IPersistentStreaming_ConsumerGrain : IStreaming_ConsumerGrain
    {
    }

    public interface IStreaming_ProducerConsumerGrain : IGrainWithIntegerKey, IStreaming_ProducerGrain, IStreaming_ConsumerGrain
    {
    }

    public interface IStreaming_Reentrant_ProducerConsumerGrain : IGrainWithIntegerKey, IStreaming_ProducerGrain, IStreaming_ConsumerGrain
    {
    }

    public interface IStreaming_ImplicitlySubscribedConsumerGrain : IGrainWithIntegerKey, IStreaming_ConsumerGrain
    {
    }

    public interface IStreaming_ImplicitlySubscribedGenericConsumerGrain<T> : IGrainWithIntegerKey, IStreaming_ConsumerGrain
    {
    }


    //------- STATE interfaces ----//

    [Serializable]
    [GenerateSerializer]
    public class Streaming_ProducerGrain_State
    {
        [Id(0)]
        public List<IProducerObserver> Producers { get; set; }
    }

    [Serializable]
    [GenerateSerializer]
    public class Streaming_ConsumerGrain_State
    {
        [Id(0)]
        public List<IConsumerObserver> Consumers { get; set; }
    }

    [Serializable]
    [GenerateSerializer]
    public class Streaming_ProducerConsumerGrain_State
    {
        [Id(0)]
        public List<IProducerObserver> Producers { get; set; }
        [Id(1)]
        public List<IConsumerObserver> Consumers { get; set; }
    }

    //------- POCO interfaces for objects that implement the actual test logic ----///

    public interface IProducerObserver
    {
        void BecomeProducer(Guid streamId, IStreamProvider streamProvider, string streamNamespace);
        void RenewProducer(ILogger logger, IStreamProvider streamProvider);
        Task StopBeingProducer();
        Task ProduceSequentialSeries(int count);
        Task ProduceParallelSeries(int count);
        Task ProducePeriodicSeries(Func<Func<object, Task>, IDisposable> createTimerFunc, int count);
        Task<int> ExpectedItemsProduced { get; }
        Task<int> ItemsProduced { get; }
        Task AddNewConsumerGrain(Guid consumerGrainId);
        Task<int> ProducerCount { get; }
        Task VerifyFinished();
        string ProviderName { get; }
    }

    public interface IConsumerObserver
    {
        Task BecomeConsumer(Guid streamId, IStreamProvider streamProvider, string streamNamespace);
        Task RenewConsumer(ILogger logger, IStreamProvider streamProvider);
        Task StopBeingConsumer(IStreamProvider streamProvider);
        Task<int> ItemsConsumed { get; }
        Task<int> ConsumerCount { get; }
        string ProviderName { get; }
    }    
}