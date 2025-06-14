using System;
using Microsoft.Extensions.Logging;
using Forkleans.Providers.Streams.Common;
using Forkleans.Configuration;

namespace Forkleans.Streaming.EventHubs
{
    /// <summary>
    /// Cache pressure monitor whose back pressure algorithm is based on averaging pressure value
    /// over all pressure contribution
    /// </summary>
    public partial class AveragingCachePressureMonitor : ICachePressureMonitor
    {
        /// <summary>
        /// Cache monitor which is used to report cache related metrics
        /// </summary>
        public ICacheMonitor CacheMonitor { set; private get; }
        private static readonly TimeSpan checkPeriod = TimeSpan.FromSeconds(2);
        private readonly ILogger logger;

        private double accumulatedCachePressure;
        private double cachePressureContributionCount;
        private DateTime nextCheckedTime;
        private bool isUnderPressure;
        private readonly double flowControlThreshold;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="monitor"></param>
        public AveragingCachePressureMonitor(ILogger logger, ICacheMonitor monitor=null)
            :this(EventHubStreamCachePressureOptions.DEFAULT_AVERAGING_CACHE_PRESSURE_MONITORING_THRESHOLD, logger, monitor)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flowControlThreshold"></param>
        /// <param name="logger"></param>
        /// <param name="monitor"></param>
        public AveragingCachePressureMonitor(double flowControlThreshold, ILogger logger, ICacheMonitor monitor=null)
        {
            this.flowControlThreshold = flowControlThreshold;
            this.logger = logger;
            nextCheckedTime = DateTime.MinValue;
            isUnderPressure = false;
            this.CacheMonitor = monitor;
        }

        /// <inheritdoc />
        public void RecordCachePressureContribution(double cachePressureContribution)
        {
            // Weight unhealthy contributions thrice as much as healthy ones.
            // This is a crude compensation for the fact that healthy consumers wil consume more often than unhealthy ones.
            double weight = cachePressureContribution < flowControlThreshold ? 1.0 : 3.0;
            accumulatedCachePressure += cachePressureContribution * weight;
            cachePressureContributionCount += weight;
        }

        /// <inheritdoc />
        public bool IsUnderPressure(DateTime utcNow)
        {
            if (nextCheckedTime < utcNow)
            {
                CalculatePressure();
                nextCheckedTime = utcNow + checkPeriod;
            }
            return isUnderPressure;
        }

        private void CalculatePressure()
        {
            // if we don't have any contributions, don't change status
            if (cachePressureContributionCount < 0.5)
            {
                // after 5 checks with no contributions, check anyway
                cachePressureContributionCount += 0.1;
                return;
            }

            double pressure = accumulatedCachePressure / cachePressureContributionCount;
            bool wasUnderPressure = isUnderPressure;
            isUnderPressure = pressure > flowControlThreshold;
            // If we changed state, log
            if (isUnderPressure != wasUnderPressure)
            {
                this.CacheMonitor?.TrackCachePressureMonitorStatusChange(this.GetType().Name, isUnderPressure, cachePressureContributionCount, pressure, this.flowControlThreshold);
                if (isUnderPressure)
                {
                    LogDebugIngestingMessagesTooFast(accumulatedCachePressure, cachePressureContributionCount, pressure, flowControlThreshold);
                }
                else
                {
                    LogDebugMessageIngestionIsHealthy(accumulatedCachePressure, cachePressureContributionCount, pressure, flowControlThreshold);
                }
            }
            cachePressureContributionCount = 0.0;
            accumulatedCachePressure = 0.0;
        }

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Ingesting messages too fast. Throttling message reading. AccumulatedCachePressure: {AccumulatedCachePressure}, Contributions: {Contributions}, AverageCachePressure: {AverageCachePressure}, Threshold: {FlowControlThreshold}"
        )]
        private partial void LogDebugIngestingMessagesTooFast(double accumulatedCachePressure, double contributions, double averageCachePressure, double flowControlThreshold);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Message ingestion is healthy. AccumulatedCachePressure: {AccumulatedCachePressure}, Contributions: {Contributions}, AverageCachePressure: {AverageCachePressure}, Threshold: {FlowControlThreshold}"
        )]
        private partial void LogDebugMessageIngestionIsHealthy(double accumulatedCachePressure, double contributions, double averageCachePressure, double flowControlThreshold);
    }
}
