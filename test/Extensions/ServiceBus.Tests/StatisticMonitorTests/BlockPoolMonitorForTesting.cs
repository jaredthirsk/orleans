using Forkleans.Providers.Streams.Common;

namespace ServiceBus.Tests.MonitorTests
{
    public class BlockPoolMonitorForTesting : IBlockPoolMonitor
    {
        public ObjectPoolMonitorCounters CallCounters { get; } = new ObjectPoolMonitorCounters();
 
        public void TrackMemoryAllocated(long allocatedMemoryInByte)
        {
            Interlocked.Increment(ref this.CallCounters.TrackObjectAllocatedByCacheCallCounter);
        }

        public void TrackMemoryReleased(long releasedMemoryInByte)
        {
            Interlocked.Increment(ref this.CallCounters.TrackObjectReleasedFromCacheCallCounter);
        }

        public void Report(long totalMemoryInByte, long availableMemoryInByte, long claimedMemoryInByte)
        {
            Interlocked.Increment(ref this.CallCounters.ReportCallCounter);
        }
    }

    [Serializable]
    [Forkleans.GenerateSerializer]
    public class ObjectPoolMonitorCounters
    {
        [Forkleans.Id(0)]
        public int TrackObjectAllocatedByCacheCallCounter;
        [Forkleans.Id(1)]
        public int TrackObjectReleasedFromCacheCallCounter;
        [Forkleans.Id(2)]
        public int ReportCallCounter;
    }
}
