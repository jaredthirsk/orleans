using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Configuration;
using Forkleans.Internal;

namespace Forkleans.Runtime.Messaging
{
    internal sealed partial class ConnectionManager
    {
        [ThreadStatic]
        private static uint nextConnection;

        private readonly ConcurrentDictionary<SiloAddress, ConnectionEntry> connections = new();
        private readonly ConnectionOptions connectionOptions;
        private readonly ConnectionFactory connectionFactory;
        private readonly NetworkingTrace trace;
        private readonly CancellationTokenSource shutdownCancellation = new();
        private readonly object lockObj = new object();
        private readonly TaskCompletionSource<int> closedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ConnectionManager(
            IOptions<ConnectionOptions> connectionOptions,
            ConnectionFactory connectionFactory,
            NetworkingTrace trace)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace));
            this.connectionOptions = connectionOptions.Value;
            this.connectionFactory = connectionFactory;
            this.trace = trace;
        }

        public int ConnectionCount => connections.Sum(e => e.Value.Connections.Length);

        public Task Closed => this.closedTaskCompletionSource.Task;

        public List<SiloAddress> GetConnectedAddresses() => connections.Select(i => i.Key).ToList();

        public ValueTask<Connection> GetConnection(SiloAddress endpoint)
        {
            if (this.connections.TryGetValue(endpoint, out var entry) && entry.NextConnection() is { } connection)
            {
                if (!entry.HasSufficientConnections(connectionOptions) && entry.PendingConnection is null)
                {
                    this.GetConnectionAsync(endpoint).Ignore();
                }

                // Return the existing connection.
                return new(connection);
            }

            // Start a new connection attempt since there are no suitable connections.
            return new(this.GetConnectionAsync(endpoint));
        }

        public bool TryGetConnection(SiloAddress endpoint, out Connection connection)
        {
            if (this.connections.TryGetValue(endpoint, out var entry) && entry.NextConnection() is { } c)
            {
                connection = c;
                return true;
            }

            connection = null;
            return false;
        }

        private async Task<Connection> GetConnectionAsync(SiloAddress endpoint)
        {
            await Task.Yield();
            while (true)
            {
                if (this.shutdownCancellation.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Shutting down");
                }

                Task pendingAttempt;
                lock (this.lockObj)
                {
                    var entry = this.GetOrCreateEntry(endpoint);
                    entry.RemoveDefunct();

                    // If there are sufficient connections available then return an existing connection.
                    if (entry.HasSufficientConnections(connectionOptions) && entry.NextConnection() is { } connection)
                    {
                        return connection;
                    }

                    var remainingDelay = entry.GetRemainingRetryDelay(connectionOptions);
                    if (remainingDelay.Ticks > 0)
                    {
                        throw new ConnectionFailedException($"Unable to connect to {endpoint}, will retry after {remainingDelay.TotalMilliseconds}ms");
                    }

                    // If there is no pending attempt then start one, otherwise the pending attempt will be awaited before reevaluating.
                    pendingAttempt = entry.PendingConnection ??= ConnectAsync(endpoint, entry);
                }

                await pendingAttempt;
            }
        }

        private void OnConnectionFailed(ConnectionEntry entry)
        {
            var lastFailure = DateTime.UtcNow;
            lock (this.lockObj)
            {
                if (entry.LastFailure < lastFailure) entry.LastFailure = lastFailure;
                entry.PendingConnection = null;
                entry.RemoveDefunct();
            }
        }

        public void OnConnected(SiloAddress address, Connection connection) => OnConnected(address, connection, null);

        private void OnConnected(SiloAddress address, Connection connection, ConnectionEntry entry)
        {
            lock (this.lockObj)
            {
                entry ??= GetOrCreateEntry(address);
                entry.Connections = entry.Connections.Contains(connection) ? entry.Connections : entry.Connections.Add(connection);
                entry.LastFailure = default;
                entry.PendingConnection = null;
            }

            LogInformationConnectionEstablished(this.trace, connection, address);
        }

        public void OnConnectionTerminated(SiloAddress address, Connection connection, Exception exception)
        {
            if (connection is null) return;

            lock (this.lockObj)
            {
                if (this.connections.TryGetValue(address, out var entry))
                {
                    entry.Connections = entry.Connections.Remove(connection);

                    if (entry.Connections.Length == 0 && entry.PendingConnection is null)
                    {
                        // Remove the entire entry.
                        this.connections.TryRemove(address, out _);
                    }
                    else
                    {
                        entry.RemoveDefunct();
                    }
                }
            }

            if (exception != null && !this.shutdownCancellation.IsCancellationRequested)
            {
                LogWarningConnectionTerminated(this.trace, exception, connection);
            }
            else
            {
                LogDebugConnectionClosed(this.trace, connection);
            }
        }

        private ConnectionEntry GetOrCreateEntry(SiloAddress address) => connections.GetOrAdd(address, _ => new());

        private async Task<Connection> ConnectAsync(SiloAddress address, ConnectionEntry entry)
        {
            await Task.Yield();
            CancellationTokenSource openConnectionCancellation = default;

            try
            {
                LogInformationEstablishingConnection(this.trace, address);

                // Cancel pending connection attempts either when the host terminates or after the configured time limit.
                openConnectionCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.shutdownCancellation.Token, default);
                openConnectionCancellation.CancelAfter(this.connectionOptions.OpenConnectionTimeout);

                var connection = await this.connectionFactory.ConnectAsync(address, openConnectionCancellation.Token);

                LogInformationConnectedToEndpoint(this.trace, address);

                this.StartConnection(address, connection);

                await connection.Initialized.WaitAsync(openConnectionCancellation.Token);
                this.OnConnected(address, connection, entry);

                return connection;
            }
            catch (Exception exception)
            {
                this.OnConnectionFailed(entry);

                LogWarningConnectionAttemptFailed(this.trace, exception, address);

                if (exception is OperationCanceledException && openConnectionCancellation?.IsCancellationRequested == true && !shutdownCancellation.IsCancellationRequested)
                    throw new ConnectionFailedException($"Connection attempt to endpoint {address} timed out after {connectionOptions.OpenConnectionTimeout}");

                throw new ConnectionFailedException(
                    $"Unable to connect to endpoint {address}. See {nameof(exception.InnerException)}", exception);
            }
            finally
            {
                openConnectionCancellation?.Dispose();
            }
        }

        public async Task CloseAsync(SiloAddress endpoint)
        {
            ImmutableArray<Connection> connections;
            lock (this.lockObj)
            {
                if (!this.connections.TryGetValue(endpoint, out var entry))
                {
                    return;
                }

                connections = entry.Connections;
                if (entry.PendingConnection is null)
                {
                    this.connections.TryRemove(endpoint, out _);
                }
            }

            if (connections.Length == 1)
            {
                await connections[0].CloseAsync(exception: null);
            }
            else if (!connections.IsEmpty)
            {
                var closeTasks = new List<Task>();
                foreach (var connection in connections)
                {
                    try
                    {
                        closeTasks.Add(connection.CloseAsync(exception: null));
                    }
                    catch
                    {
                    }
                }

                await Task.WhenAll(closeTasks);
            }
        }

        public async Task Close(CancellationToken ct)
        {
            try
            {
                LogDebugShuttingDownConnections(this.trace);

                this.shutdownCancellation.Cancel(throwOnFirstException: false);

                var cycles = 0;
                for (var closeTasks = new List<Task>(); ; closeTasks.Clear())
                {
                    var pendingConnections = false;
                    foreach (var kv in connections)
                    {
                        pendingConnections |= kv.Value.PendingConnection != null;
                        foreach (var connection in kv.Value.Connections)
                        {
                            try
                            {
                                closeTasks.Add(connection.CloseAsync(exception: null));
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (closeTasks.Count > 0)
                    {
                        await Task.WhenAll(closeTasks).WaitAsync(ct).SuppressThrowing();
                        if (ct.IsCancellationRequested) break;
                    }
                    else if (!pendingConnections) break;

                    await Task.Delay(10);
                    if (++cycles > 100 && cycles % 500 == 0 && this.ConnectionCount is var remaining and > 0)
                    {
                        LogWarningWaitingForConnectionsToTerminate(this.trace, remaining);
                    }
                }
            }
            catch (Exception exception)
            {
                LogWarningExceptionDuringShutdown(this.trace, exception);
            }
            finally
            {
                this.closedTaskCompletionSource.TrySetResult(0);
            }
        }

        private void StartConnection(SiloAddress address, Connection connection)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state =>
            {
                var (t, address, connection) = ((ConnectionManager, SiloAddress, Connection))state;
                t.RunConnectionAsync(address, connection).Ignore();
            }, (this, address, connection));
        }

        private async Task RunConnectionAsync(SiloAddress address, Connection connection)
        {
            Exception error = default;
            try
            {
                using (this.BeginConnectionScope(connection))
                {
                    await connection.Run();
                }
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                this.OnConnectionTerminated(address, connection, error);
            }
        }

        private IDisposable BeginConnectionScope(Connection connection)
        {
            if (this.trace.IsEnabled(LogLevel.Critical))
            {
                return this.trace.BeginScope(new ConnectionLogScope(connection));
            }

            return null;
        }

        private sealed class ConnectionEntry
        {
            public Task PendingConnection { get; set; }
            public DateTime LastFailure { get; set; }
            public ImmutableArray<Connection> Connections { get; set; } = ImmutableArray<Connection>.Empty;

            public TimeSpan GetRemainingRetryDelay(ConnectionOptions options)
            {
                var lastFailure = this.LastFailure;
                if (lastFailure.Ticks > 0)
                {
                    var retryAfter = lastFailure + options.ConnectionRetryDelay;
                    var remainingDelay = retryAfter - DateTime.UtcNow;
                    if (remainingDelay.Ticks > 0)
                    {
                        return remainingDelay;
                    }
                }

                return default;
            }

            public bool HasSufficientConnections(ConnectionOptions options) => Connections.Length >= options.ConnectionsPerEndpoint;

            public Connection NextConnection()
            {
                var connections = this.Connections;
                if (connections.IsEmpty)
                {
                    return null;
                }

                var result = connections.Length == 1 ? connections[0] : connections[(int)(++nextConnection % (uint)connections.Length)];
                return result.IsValid ? result : null;
            }

            public void RemoveDefunct() => Connections = Connections.RemoveAll(c => !c.IsValid);
        }

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = "Connection {Connection} established with {Silo}"
        )]
        private static partial void LogInformationConnectionEstablished(ILogger logger, Connection connection, SiloAddress silo);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Connection {Connection} terminated"
        )]
        private static partial void LogWarningConnectionTerminated(ILogger logger, Exception exception, Connection connection);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Connection {Connection} closed"
        )]
        private static partial void LogDebugConnectionClosed(ILogger logger, Connection connection);

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = "Establishing connection to endpoint {EndPoint}"
        )]
        private static partial void LogInformationEstablishingConnection(ILogger logger, SiloAddress endPoint);

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = "Connected to endpoint {EndPoint}"
        )]
        private static partial void LogInformationConnectedToEndpoint(ILogger logger, SiloAddress endPoint);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Connection attempt to endpoint {EndPoint} failed"
        )]
        private static partial void LogWarningConnectionAttemptFailed(ILogger logger, Exception exception, SiloAddress endPoint);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Shutting down connections"
        )]
        private static partial void LogDebugShuttingDownConnections(ILogger logger);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Waiting for {NumRemaining} connections to terminate"
        )]
        private static partial void LogWarningWaitingForConnectionsToTerminate(ILogger logger, int numRemaining);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Exception during shutdown"
        )]
        private static partial void LogWarningExceptionDuringShutdown(ILogger logger, Exception exception);
    }
}
