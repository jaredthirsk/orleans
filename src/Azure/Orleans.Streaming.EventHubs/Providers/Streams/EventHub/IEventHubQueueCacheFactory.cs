using Microsoft.Extensions.Logging;
using Forkleans.Streams;

namespace Forkleans.Streaming.EventHubs
{
    /// <summary>
    /// Factory responsible for creating a message cache for an EventHub partition.
    /// </summary>
    public interface IEventHubQueueCacheFactory
    {
        /// <summary>
        /// Function used to create a IEventHubQueueCache
        /// </summary>
        IEventHubQueueCache CreateCache(string partition, IStreamQueueCheckpointer<string> checkpointer, ILoggerFactory loggerFactory);
    }
}