#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Configuration;
using Forkleans.Internal;
using Forkleans.Runtime.Messaging;

namespace Forkleans.Runtime.MembershipService
{
    internal interface ILocalSiloHealthMonitor
    {
        /// <summary>
        /// Returns the local health degradation score, which is a value between 0 (healthy) and <see cref="LocalSiloHealthMonitor.MaxScore"/> (unhealthy).
        /// </summary>
        /// <param name="checkTime">The time which the check is taking place.</param>
        /// <returns>The local health degradation score, which is a value between 0 (healthy) and <see cref="LocalSiloHealthMonitor.MaxScore"/> (unhealthy).</returns>
        int GetLocalHealthDegradationScore(DateTime checkTime);

        /// <summary>
        /// The most recent list of detected health issues.
        /// </summary>
        ImmutableArray<string> Complaints { get; }
    }

    /// <summary>
    /// Monitors the health of the local node using a combination of heuristics to create a health degradation score which
    /// is exposed as a boolean value: whether or not the local node's health is degraded.
    /// </summary>
    /// <remarks>
    /// The primary goal of this functionality is to passify degraded nodes so that they do not evict healthy nodes.
    /// This functionality is inspired by the Lifeguard paper (https://arxiv.org/abs/1707.00788), which is a set of extensions
    /// to the SWIM membership algorithm (https://research.cs.cornell.edu/projects/Quicksilver/public_pdfs/SWIM.pdf). Orleans
    /// uses a strong consistency membership algorithm, and not all of the Lifeguard extensions to SWIM apply to Orleans'
    /// membership algorithm (refutation, for example).
    /// The monitor implements the following heuristics:
    /// <list type="bullet">
    ///   <item><description>Check that this silos is marked as active in membership.</description></item>
    ///   <item><description>Check that no other silo suspects this silo.</description></item>
    ///   <item><description>Check for recently received successful ping responses.</description></item>
    ///   <item><description>Check for recently received ping requests.</description></item>
    ///   <item><description>Check that the .NET Thread Pool is able to process work items within one second.</description></item>
    ///   <item><description>Check that local async timers have been firing on-time (within 3 seconds of their due time).</description></item>
    /// </list>
    /// </remarks>
    internal partial class LocalSiloHealthMonitor : ILifecycleParticipant<ISiloLifecycle>, ILifecycleObserver, ILocalSiloHealthMonitor
    {
        private const int MaxScore = 8;
        private readonly List<IHealthCheckParticipant> _healthCheckParticipants;
        private readonly MembershipTableManager _membershipTableManager;
        private readonly ClusterHealthMonitor _clusterHealthMonitor;
        private readonly ILocalSiloDetails _localSiloDetails;
        private readonly ILogger<LocalSiloHealthMonitor> _lo;
        private readonly ClusterMembershipOptions _clusterMembershipOptions;
        private readonly IAsyncTimer _degradationCheckTimer;
        private readonly ThreadPoolMonitor _threadPoolMonitor;
        private readonly ProbeRequestMonitor _probeRequestMonitor;
        private ValueStopwatch _clusteredDuration;
        private Task? _runTask;
        private bool _isActive;
        private DateTime _lastHealthCheckTime;

        public LocalSiloHealthMonitor(
            IEnumerable<IHealthCheckParticipant> healthCheckParticipants,
            MembershipTableManager membershipTableManager,
            ConnectionManager connectionManager,
            ClusterHealthMonitor clusterHealthMonitor,
            ILocalSiloDetails localSiloDetails,
            ILogger<LocalSiloHealthMonitor> log,
            IOptions<ClusterMembershipOptions> clusterMembershipOptions,
            IAsyncTimerFactory timerFactory,
            ILoggerFactory loggerFactory,
            ProbeRequestMonitor probeRequestMonitor)
        {
            _healthCheckParticipants = healthCheckParticipants.ToList();
            _membershipTableManager = membershipTableManager;
            _clusterHealthMonitor = clusterHealthMonitor;
            _localSiloDetails = localSiloDetails;
            _lo = log;
            _probeRequestMonitor = probeRequestMonitor;
            _clusterMembershipOptions = clusterMembershipOptions.Value;
            _degradationCheckTimer = timerFactory.Create(
                _clusterMembershipOptions.LocalHealthDegradationMonitoringPeriod,
                nameof(LocalSiloHealthMonitor));
            _threadPoolMonitor = new ThreadPoolMonitor(loggerFactory.CreateLogger<ThreadPoolMonitor>());
        }

        /// <inheritdoc />
        public ImmutableArray<string> Complaints { get; private set; } = ImmutableArray<string>.Empty;

        /// <inheritdoc />
        public int GetLocalHealthDegradationScore(DateTime checkTime) => GetLocalHealthDegradationScore(checkTime, null);

        /// <summary>
        /// Returns the local health degradation score, which is a value between 0 (healthy) and <see cref="MaxScore"/> (unhealthy).
        /// </summary>
        /// <param name="checkTime">The time which the check is taking place.</param>
        /// <param name="complaints">If not null, will be populated with the current set of detected health issues.</param>
        /// <returns>The local health degradation score, which is a value between 0 (healthy) and <see cref="MaxScore"/> (unhealthy).</returns>
        public int GetLocalHealthDegradationScore(DateTime checkTime, List<string>? complaints)
        {
            var score = 0;
            score += CheckSuspectingNodes(checkTime, complaints);
            score += CheckLocalHealthCheckParticipants(checkTime, complaints);
            score += CheckThreadPoolQueueDelay(checkTime, complaints);

            if (_isActive)
            {
                var membershipSnapshot = _membershipTableManager.MembershipTableSnapshot;
                if (membershipSnapshot.ActiveNodeCount <= 1)
                {
                    _clusteredDuration.Reset();
                }
                else if (!_clusteredDuration.IsRunning)
                {
                    _clusteredDuration.Restart();
                }

                // Only consider certain checks if the silo has been a member of a multi-silo cluster for a certain period.
                var recencyWindow = _clusterMembershipOptions.ProbeTimeout.Multiply(_clusterMembershipOptions.NumMissedProbesLimit);
                if (_clusteredDuration.Elapsed > recencyWindow)
                {
                    score += CheckReceivedProbeResponses(checkTime, complaints);
                    score += CheckReceivedProbeRequests(checkTime, complaints);
                }
            }

            // Clamp the score between 0 and the maximum allowed score.
            score = Math.Max(0, Math.Min(MaxScore, score));
            return score;
        }

        private int CheckThreadPoolQueueDelay(DateTime checkTime, List<string>? complaints)
        {
            var threadPoolDelaySeconds = _threadPoolMonitor.MeasureQueueDelay().TotalSeconds;

            if ((int)threadPoolDelaySeconds >= 1)
            {
                // Log as an error if the delay is massive.
                var logLevel = (int)threadPoolDelaySeconds >= 10 ? LogLevel.Error : LogLevel.Warning;
                LogThreadPoolDelay(logLevel, threadPoolDelaySeconds);

                complaints?.Add(
                    $".NET Thread Pool is exhibiting delays of {threadPoolDelaySeconds}s. This can indicate .NET Thread Pool starvation, very long .NET GC pauses, or other runtime or machine pauses.");
            }

            // Each second of delay contributes to the score.
            return (int)threadPoolDelaySeconds;
        }

        private int CheckSuspectingNodes(DateTime now, List<string>? complaints)
        {
            var score = 0;
            var membershipSnapshot = _membershipTableManager.MembershipTableSnapshot;
            if (membershipSnapshot.Entries.TryGetValue(_localSiloDetails.SiloAddress, out var membershipEntry))
            {
                if (membershipEntry.Status != SiloStatus.Active)
                {
                    LogSiloNotActive(membershipEntry.Status);
                    complaints?.Add($"This silo is not active (Status: {membershipEntry.Status}) and is therefore not healthy.");

                    score = MaxScore;
                }

                // Check if there are valid votes against this node.
                var expiration = _clusterMembershipOptions.DeathVoteExpirationTimeout;
                var freshVotes = membershipEntry.GetFreshVotes(now, expiration);
                foreach (var vote in freshVotes)
                {
                    if (membershipSnapshot.GetSiloStatus(vote.Item1) == SiloStatus.Active)
                    {
                        LogSiloSuspected(vote.Item1, vote.Item2);
                        complaints?.Add($"Silo {vote.Item1} recently suspected this silo is dead at {vote.Item2}.");

                        ++score;
                    }
                }
            }
            else
            {
                LogMembershipEntryNotFound();
                complaints?.Add("Could not find a membership entry for this silo");

                score = MaxScore;
            }

            return score;
        }

        private int CheckReceivedProbeRequests(DateTime now, List<string>? complaints)
        {
            // Have we received ping REQUESTS from other nodes?
            var score = 0;
            var membershipSnapshot = _membershipTableManager.MembershipTableSnapshot;

            // Only consider recency of the last received probe request if there is more than one other node.
            // Otherwise, it may fail to vote another node dead in a one or two node cluster.
            if (membershipSnapshot.ActiveNodeCount > 2)
            {
                var sinceLastProbeRequest = _probeRequestMonitor.ElapsedSinceLastProbeRequest;
                var recencyWindow = _clusterMembershipOptions.ProbeTimeout.Multiply(_clusterMembershipOptions.NumMissedProbesLimit);
                if (!sinceLastProbeRequest.HasValue)
                {
                    LogNoProbeRequests();
                    complaints?.Add("This silo has not received any probe requests");
                    ++score;
                }
                else if (sinceLastProbeRequest.Value > recencyWindow)
                {
                    // This node has not received a successful ping response since the window began.
                    var lastRequestTime = now - sinceLastProbeRequest.Value;
                    LogNoRecentProbeRequest(lastRequestTime);
                    complaints?.Add($"This silo has not received a probe request since {lastRequestTime}");
                    ++score;
                }
            }

            return score;
        }

        private int CheckReceivedProbeResponses(DateTime now, List<string>? complaints)
        {
            // Determine how recently the latest successful ping response was received.
            var siloMonitors = _clusterHealthMonitor.SiloMonitors;
            var elapsedSinceLastResponse = default(TimeSpan?);
            foreach (var monitor in siloMonitors.Values)
            {
                var current = monitor.ElapsedSinceLastResponse;
                if (current.HasValue && (!elapsedSinceLastResponse.HasValue || current.Value < elapsedSinceLastResponse.Value))
                {
                    elapsedSinceLastResponse = current.Value;
                }
            }

            // Only consider recency of the last successful ping if this node is monitoring more than one other node.
            // Otherwise, it may fail to vote another node dead in a one or two node cluster.
            int score = 0;
            if (siloMonitors.Count > 1)
            {
                var recencyWindow = _clusterMembershipOptions.ProbeTimeout.Multiply(_clusterMembershipOptions.NumMissedProbesLimit);
                if (!elapsedSinceLastResponse.HasValue)
                {
                    LogNoProbeResponses();
                    complaints?.Add("This silo has not received any successful probe responses");
                    ++score;
                }
                else if (elapsedSinceLastResponse.Value > recencyWindow)
                {
                    // This node has not received a successful ping response since the window began.
                    LogNoRecentProbeResponse(elapsedSinceLastResponse.Value);
                    complaints?.Add($"This silo has not received a successful probe response since {elapsedSinceLastResponse.Value}");
                    ++score;
                }
            }

            return score;
        }

        private int CheckLocalHealthCheckParticipants(DateTime now, List<string>? complaints)
        {
            // Check for execution delays and other local health warning signs.
            var score = 0;
            foreach (var participant in _healthCheckParticipants)
            {
                try
                {
                    if (!participant.CheckHealth(_lastHealthCheckTime, out var reason))
                    {
                        LogHealthCheckParticipantUnhealthy(participant.GetType(), reason);
                        complaints?.Add($"Health check participant {participant.GetType()} is reporting that it is unhealthy with complaint: {reason}");

                        ++score;
                    }
                }
                catch (Exception exception)
                {
                    LogHealthCheckParticipantError(exception, participant.GetType());
                    complaints?.Add($"Error checking health for participant {participant.GetType()}: {LogFormatter.PrintException(exception)}");

                    ++score;
                }
            }

            _lastHealthCheckTime = now;
            return score;
        }

        private async Task Run()
        {
            while (await _degradationCheckTimer.NextTick())
            {
                try
                {
                    var complaints = new List<string>();
                    var now = DateTime.UtcNow;
                    var score = GetLocalHealthDegradationScore(now, complaints);
                    if (score > 0)
                    {
                        var complaintsString = string.Join("\n", complaints);
                        LogSelfMonitoringDegraded(score, MaxScore, complaintsString);
                    }

                    this.Complaints = ImmutableArray.CreateRange(complaints);
                }
                catch (Exception exception)
                {
                    LogErrorMonitoringLocalSiloHealth(exception);
                }
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(ServiceLifecycleStage.Active, this);
        }

        public Task OnStart(CancellationToken ct)
        {
            _runTask = Task.Run(this.Run);
            _isActive = true;
            return Task.CompletedTask;
        }

        public async Task OnStop(CancellationToken ct)
        {
            _degradationCheckTimer.Dispose();
            _isActive = false;

            if (_runTask is Task task)
            {
                await task.WaitAsync(ct).SuppressThrowing();
            }
        }

        /// <summary>
        /// Measures queue delay on the .NET <see cref="ThreadPool"/>.
        /// </summary>
        private class ThreadPoolMonitor
        {
            private static readonly WaitCallback Callback = state => ((ThreadPoolMonitor)state!).Execute();
            private readonly object _lockObj = new object();
            private readonly ILogger<ThreadPoolMonitor> _log;
            private bool _scheduled;
            private TimeSpan _lastQueueDelay;
            private ValueStopwatch _queueDelay;

            public ThreadPoolMonitor(ILogger<ThreadPoolMonitor> log)
            {
                _log = log;
            }

            public TimeSpan MeasureQueueDelay()
            {
                bool shouldSchedule;
                TimeSpan delay;
                lock (_lockObj)
                {
                    var currentQueueDelay = _queueDelay.Elapsed;
                    delay = currentQueueDelay > _lastQueueDelay ? currentQueueDelay : _lastQueueDelay;

                    if (!_scheduled)
                    {
                        _scheduled = true;
                        shouldSchedule = true;
                        _queueDelay.Restart();
                    }
                    else
                    {
                        shouldSchedule = false;
                    }
                }

                if (shouldSchedule)
                {
                    _ = ThreadPool.UnsafeQueueUserWorkItem(Callback, this);
                }

                return delay;
            }

            private void Execute()
            {
                try
                {
                    lock (_lockObj)
                    {
                        _scheduled = false;
                        _queueDelay.Stop();
                        _lastQueueDelay = _queueDelay.Elapsed;
                    }
                }
                catch (Exception exception)
                {
                    _log.LogError(exception, "Exception monitoring .NET thread pool delay");
                }
            }
        }

        [LoggerMessage(
            Message = ".NET Thread Pool is exhibiting delays of {ThreadPoolQueueDelaySeconds}s. This can indicate .NET Thread Pool starvation, very long .NET GC pauses, or other runtime or machine pauses."
        )]
        private partial void LogThreadPoolDelay(LogLevel logLevel, double threadPoolQueueDelaySeconds);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "This silo is not active (Status: {Status}) and is therefore not healthy."
        )]
        private partial void LogSiloNotActive(SiloStatus status);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Silo {Silo} recently suspected this silo is dead at {SuspectingTime}."
        )]
        private partial void LogSiloSuspected(SiloAddress silo, DateTime suspectingTime);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Could not find a membership entry for this silo"
        )]
        private partial void LogMembershipEntryNotFound();

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "This silo has not received any probe requests"
        )]
        private partial void LogNoProbeRequests();

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "This silo has not received a probe request since {LastProbeRequest}"
        )]
        private partial void LogNoRecentProbeRequest(DateTime lastProbeRequest);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "This silo has not received any successful probe responses"
        )]
        private partial void LogNoProbeResponses();

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "This silo has not received a successful probe response since {LastSuccessfulResponse}"
        )]
        private partial void LogNoRecentProbeResponse(TimeSpan lastSuccessfulResponse);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Health check participant {Participant} is reporting that it is unhealthy with complaint: {Reason}"
        )]
        private partial void LogHealthCheckParticipantUnhealthy(Type participant, string reason);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Error checking health for {Participant}"
        )]
        private partial void LogHealthCheckParticipantError(Exception exception, Type participant);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Self-monitoring determined that local health is degraded. Degradation score is {Score}/{MaxScore} (lower is better). Complaints: {Complaints}"
        )]
        private partial void LogSelfMonitoringDegraded(int score, int maxScore, string complaints);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Error while monitoring local silo health"
        )]
        private partial void LogErrorMonitoringLocalSiloHealth(Exception exception);
    }
}
