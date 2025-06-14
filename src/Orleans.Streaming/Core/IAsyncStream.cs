using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forkleans.Runtime;

namespace Forkleans.Streams
{
    /// <summary>
    /// This interface represents an object that serves as a distributed rendezvous between producers and consumers.
    /// It is similar to a Reactive Framework <code>Subject</code> and implements
    /// <code>IObserver</code> nor <code>IObservable</code> interfaces.
    /// </summary>
    /// <typeparam name="T">The type of object that flows through the stream.</typeparam>
    public interface IAsyncStream<T> :
        IAsyncStream,
        IEquatable<IAsyncStream<T>>, IComparable<IAsyncStream<T>>, // comparison
        IAsyncObservable<T>, IAsyncBatchObservable<T>, // observables
        IAsyncBatchProducer<T> // observers
    {
        /// <summary>
        /// Retrieves a list of all active subscriptions created by the caller for this stream.
        /// </summary>
        /// <returns></returns>
        Task<IList<StreamSubscriptionHandle<T>>> GetAllSubscriptionHandles();
    }

    /// <summary>
    /// This interface represents an object that serves as a distributed rendezvous between producers and consumers.
    /// It is similar to a Reactive Framework <code>Subject</code> and implements
    /// <code>IObserver</code> nor <code>IObservable</code> interfaces.
    /// </summary>
    public interface IAsyncStream
    {
        /// <summary>
        /// Gets a value indicating whether this is a rewindable stream - supports subscribing from previous point in time.
        /// </summary>
        /// <returns>True if this is a rewindable stream, false otherwise.</returns>
        bool IsRewindable { get; }

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        /// <value>The name of the provider.</value>
        string ProviderName { get; }

        /// <summary>
        /// Gets the stream identifier.
        /// </summary>
        /// <value>The stream identifier.</value>
        StreamId StreamId { get; }
    }
}
