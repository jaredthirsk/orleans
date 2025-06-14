using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Forkleans.TestingHost;

/// <summary>
/// Default <see cref="ITestClusterPortAllocator"/> implementation, which tries to allocate unused ports.
/// </summary>
public class TestClusterPortAllocator : ITestClusterPortAllocator
{
    private bool _disposed;
    private readonly object _lockObj = new();
    private readonly Dictionary<int, string> _allocatedPorts = [];

    /// <inheritdoc />
    public (int, int) AllocateConsecutivePortPairs(int numPorts = 5)
    {
        // Evaluate current system tcp connections
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

        // each returned port in the pair will have to have at least this amount of available ports following it

        return (GetAvailableConsecutiveServerPorts(tcpConnInfoArray, 22300, 30000, numPorts),
            GetAvailableConsecutiveServerPorts(tcpConnInfoArray, 40000, 50000, numPorts));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        lock (_lockObj)
        {
            if (_disposed)
            {
                return;
            }

            foreach (var pair in _allocatedPorts)
            {
                MutexManager.Instance.SignalRelease(pair.Value);
            }

            _allocatedPorts.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TestClusterPortAllocator"/> class.
    /// </summary>
    ~TestClusterPortAllocator()
    {
        Dispose(false);
    }

    private int GetAvailableConsecutiveServerPorts(IPEndPoint[] tcpConnInfoArray, int portStartRange, int portEndRange, int consecutivePortsToCheck)
    {
        const int MaxAttempts = 100;

        var allocations = new List<(int Port, string Mutex)>();

        for (var attempts = 0; attempts < MaxAttempts; attempts++)
        {
            var basePort = Random.Shared.Next(portStartRange, portEndRange);

            // get ports in buckets, so we don't interfere with parallel runs of this same function
            basePort = basePort - basePort % consecutivePortsToCheck;
            var endPort = basePort + consecutivePortsToCheck;

            // make sure none of the ports in the sub range are in use
            if (tcpConnInfoArray.All(endpoint => endpoint.Port < basePort || endpoint.Port >= endPort))
            {
                var portsAvailable = true;
                for (var i = 0; i < consecutivePortsToCheck; i++)
                {
                    var port = basePort + i;
                    try
                    {
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                    }
                    catch (SocketException)
                    {
                        portsAvailable = false;
                        break;
                    }
                }

                if (!portsAvailable)
                {
                    continue;
                }

                for (var i = 0; i < consecutivePortsToCheck; i++)
                {
                    var port = basePort + i;
                    var name = $"Global.TestCluster.{port.ToString(CultureInfo.InvariantCulture)}";
                    if (MutexManager.Instance.Acquire(name))
                    {
                        allocations.Add((port, name));
                    }
                    else
                    {
                        foreach (var allocation in allocations)
                        {
                            MutexManager.Instance.SignalRelease(allocation.Mutex);
                        }

                        allocations.Clear();
                        break;
                    }
                }

                if (allocations.Count == 0)
                {
                    // Try a different range.
                    continue;
                }

                lock (_lockObj)
                {
                    foreach (var allocation in allocations)
                    {
                        _allocatedPorts[allocation.Port] = allocation.Mutex;
                    }
                }

                return basePort;
            }
        }

        throw new InvalidOperationException("Cannot find enough free ports to spin up a cluster");
    }

    private class MutexManager
    {
        private readonly Dictionary<string, Mutex> _mutexes = [];
        private readonly BlockingCollection<Action> _workItems = [];
        private readonly Thread _thread;

        public static MutexManager Instance { get; } = new MutexManager();

        private MutexManager()
        {
            _thread = new Thread(Run)
            {
                Name = "MutexManager.Worker",
                IsBackground = true,
            };
            _thread.Start();
            AppDomain.CurrentDomain.DomainUnload += this.OnAppDomainUnload;
        }

        private void OnAppDomainUnload(object sender, EventArgs e) => Shutdown();

        private void Shutdown()
        {
            _workItems.CompleteAdding();
            _thread.Join();
        }

        public bool Acquire(string name)
        {
            var result = new [] { 0 };
            var signal = new ManualResetEventSlim(initialState: false);
            _workItems.Add(() =>
            {
                try
                {
                    if (!_mutexes.TryGetValue(name, out var mutex))
                    {
                        mutex = new Mutex(false, name);
                        if (mutex.WaitOne(500))
                        {
                            // Acquired
                            _mutexes[name] = mutex;
                            Interlocked.Increment(ref result[0]);
                            return;
                        }

                        // Failed to acquire: the mutex is already held by another process.
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        finally
                        {
                            mutex.Close();
                        }
                    }

                    // Failed to acquire: the mutex is already held by this process.
                }
                finally
                {
                    signal.Set();
                }
            });

            if (!signal.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Timed out while waiting for MutexManager to acquire mutex.");
            }

            return result[0] == 1;
        }

        public void SignalRelease(string name)
        {
            if (_workItems.IsAddingCompleted) return;

            try
            {
                _workItems.Add(() =>
                {
                    if (_mutexes.Remove(name, out var value))
                    {
                        value.ReleaseMutex();
                        value.Close();
                    }
                });
            }
            catch
            {
            }
        }

        private void Run()
        {
            try
            {
                foreach (var action in _workItems.GetConsumingEnumerable())
                {
                    try
                    {
                        action();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            finally
            {
                foreach (var mutex in _mutexes.Values)
                {
                    try
                    {
                        mutex.ReleaseMutex();
                    }
                    catch { }
                    finally
                    {
                        mutex.Close();
                    }
                }

                _mutexes.Clear();
            }
        }
    }
}