using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Rpc.Configuration;
using Forkleans.Rpc.Transport;
using Forkleans.Runtime;

namespace Forkleans.Rpc
{
    /// <summary>
    /// RPC client implementation.
    /// </summary>
    internal sealed class RpcClient : IClusterClient, IRpcClient, IHostedService
    {
        private readonly ILogger<RpcClient> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RpcClientOptions _clientOptions;
        private readonly RpcTransportOptions _transportOptions;
        private readonly IRpcTransportFactory _transportFactory;
        private readonly IClusterClientLifecycle _lifecycle;
        
        private IRpcTransport _transport;
        private bool _isConnected;
        private IPEndPoint _serverEndpoint;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Protocol.RpcResponse>> _pendingRequests 
            = new ConcurrentDictionary<Guid, TaskCompletionSource<Protocol.RpcResponse>>();

        public bool IsInitialized => _isConnected;
        public IServiceProvider ServiceProvider => _serviceProvider;

        public RpcClient(
            ILogger<RpcClient> logger,
            IServiceProvider serviceProvider,
            IOptions<RpcClientOptions> clientOptions,
            IOptions<RpcTransportOptions> transportOptions,
            IRpcTransportFactory transportFactory,
            IClusterClientLifecycle lifecycle)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _clientOptions = clientOptions?.Value ?? throw new ArgumentNullException(nameof(clientOptions));
            _transportOptions = transportOptions?.Value ?? throw new ArgumentNullException(nameof(transportOptions));
            _transportFactory = transportFactory ?? throw new ArgumentNullException(nameof(transportFactory));
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RPC client {ClientId}", _clientOptions.ClientId);
            
            try
            {
                // Call ConsumeServices on the runtime client to break circular dependencies
                var runtimeClient = _serviceProvider.GetService<IRuntimeClient>() as OutsideRpcRuntimeClient;
                if (runtimeClient != null)
                {
                    runtimeClient.ConsumeServices();
                    _logger.LogDebug("ConsumeServices called on OutsideRpcRuntimeClient");
                }
                
                await (_lifecycle as ILifecycleSubject)?.OnStart(cancellationToken);
                await ConnectAsync(cancellationToken);
                _logger.LogInformation("RPC client started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RPC client");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping RPC client");
            
            try
            {
                await DisconnectAsync();
                await (_lifecycle as ILifecycleSubject)?.OnStop(cancellationToken);
                _logger.LogInformation("RPC client stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping RPC client");
                throw;
            }
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string grainClassNamePrefix = null) 
            where TGrainInterface : IGrainWithGuidKey
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string grainClassNamePrefix = null) 
            where TGrainInterface : IGrainWithIntegerKey
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string grainClassNamePrefix = null) 
            where TGrainInterface : IGrainWithStringKey
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string grainClassNamePrefix = null) 
            where TGrainInterface : IGrainWithGuidCompoundKey
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string grainClassNamePrefix = null) 
            where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(GrainId grainId) where TGrainInterface : IAddressable
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain<TGrainInterface>(grainId);
        }

        public IAddressable GetGrain(GrainId grainId)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainId);
        }

        public IAddressable GetGrain(GrainId grainId, GrainInterfaceType interfaceType)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainId, interfaceType);
        }

        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        public IGrain GetGrain(Type grainInterfaceType, string grainPrimaryKey)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey, string grainClassNamePrefix)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainInterfaceType, grainPrimaryKey, grainClassNamePrefix);
        }

        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey, string grainClassNamePrefix)
        {
            EnsureConnected();
            var grainFactory = _serviceProvider.GetRequiredService<IGrainFactory>();
            return grainFactory.GetGrain(grainInterfaceType, grainPrimaryKey, grainClassNamePrefix);
        }

        public TGrainObserverInterface CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj) 
            where TGrainObserverInterface : IGrainObserver
        {
            throw new NotSupportedException("Grain observers are not supported in RPC mode");
        }

        public void DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj) 
            where TGrainObserverInterface : IGrainObserver
        {
            // Not supported in RPC mode
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_clientOptions.ServerEndpoints.Count == 0)
            {
                throw new InvalidOperationException("No server endpoints configured");
            }

            _transport = _transportFactory.CreateTransport(_serviceProvider);
            _transport.DataReceived += OnDataReceived;
            _transport.ConnectionEstablished += OnConnectionEstablished;
            _transport.ConnectionClosed += OnConnectionClosed;

            // For now, just use the first endpoint
            // TODO: Implement round-robin or failover logic
            _serverEndpoint = _clientOptions.ServerEndpoints[0];
            
            _logger.LogInformation("Connecting to RPC server at {Endpoint}", _serverEndpoint);
            
            // Start transport (client mode will connect automatically)
            await _transport.StartAsync(_serverEndpoint, cancellationToken);
            
            // The transport's StartAsync waits for connection, so we're connected now
            _isConnected = true;
            _logger.LogDebug("Transport connected, setting _isConnected = true");
            
            // Send handshake
            await SendHandshake();
        }

        private async Task DisconnectAsync()
        {
            if (_transport != null)
            {
                _transport.DataReceived -= OnDataReceived;
                _transport.ConnectionEstablished -= OnConnectionEstablished;
                _transport.ConnectionClosed -= OnConnectionClosed;
                
                await _transport.StopAsync(CancellationToken.None);
                _transport.Dispose();
                _transport = null;
            }
            
            _isConnected = false;
        }

        private void EnsureConnected()
        {
            _logger.LogDebug("EnsureConnected called, _isConnected = {IsConnected}", _isConnected);
            if (!_isConnected)
            {
                throw new InvalidOperationException("RPC client is not connected");
            }
        }

        private void OnDataReceived(object sender, RpcDataReceivedEventArgs e)
        {
            try
            {
                _logger.LogDebug("Received {ByteCount} bytes from server", e.Data.Length);
                
                // Deserialize the message
                var messageSerializer = _serviceProvider.GetRequiredService<Protocol.RpcMessageSerializer>();
                var message = messageSerializer.DeserializeMessage(e.Data);
                _logger.LogDebug("Deserialized message type: {MessageType}", message.GetType().Name);

                // Handle different message types
                switch (message)
                {
                    case Protocol.RpcHandshake handshake:
                        _logger.LogInformation("Received handshake response from server {ServerId}", handshake.ClientId);
                        break;
                        
                    case Protocol.RpcHandshakeAck handshakeAck:
                        HandleHandshakeAck(handshakeAck);
                        break;
                        
                    case Protocol.RpcResponse response:
                        HandleResponse(response);
                        break;
                        
                    case Protocol.RpcHeartbeat heartbeat:
                        _logger.LogDebug("Received heartbeat from server {ServerId}", heartbeat.SourceId);
                        break;
                        
                    default:
                        _logger.LogWarning("Received unexpected message type: {Type}", message.GetType().Name);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing received data from server");
            }
        }

        private void HandleHandshakeAck(Protocol.RpcHandshakeAck handshakeAck)
        {
            _logger.LogInformation("Received handshake acknowledgment from server {ServerId}, manifest included: {HasManifest}", 
                handshakeAck.ServerId, handshakeAck.GrainManifest != null);
            
            // Update the client manifest provider with server's grain manifest
            if (handshakeAck.GrainManifest != null)
            {
                var manifestProvider = _serviceProvider.GetService<Hosting.RpcClientManifestProvider>();
                if (manifestProvider != null)
                {
                    manifestProvider.UpdateFromServer(handshakeAck.GrainManifest);
                    _logger.LogInformation("Updated client manifest with {GrainCount} grains and {InterfaceCount} interfaces",
                        handshakeAck.GrainManifest.GrainProperties.Count,
                        handshakeAck.GrainManifest.InterfaceProperties.Count);
                }
                else
                {
                    _logger.LogWarning("Could not find RpcClientManifestProvider to update manifest");
                }
            }
            else
            {
                _logger.LogWarning("Server handshake acknowledgment did not include grain manifest");
            }
        }

        private void HandleResponse(Protocol.RpcResponse response)
        {
            if (_pendingRequests.TryRemove(response.RequestId, out var tcs))
            {
                
                if (response.Success)
                {
                    tcs.SetResult(response);
                }
                else
                {
                    tcs.SetException(new Exception(response.ErrorMessage ?? "RPC call failed"));
                }
            }
            else
            {
                _logger.LogWarning("Received response for unknown request {RequestId}", response.RequestId);
            }
        }

        private Task SendHandshake()
        {
            var handshake = new Protocol.RpcHandshake
            {
                ClientId = _clientOptions.ClientId,
                ProtocolVersion = 1,
                Features = new[] { "basic-rpc" }
            };

            var messageSerializer = _serviceProvider.GetRequiredService<Protocol.RpcMessageSerializer>();
            var data = messageSerializer.SerializeMessage(handshake);
            
            _logger.LogInformation("Sent handshake to server");
            return _transport.SendAsync(_serverEndpoint, data, CancellationToken.None);
        }

        private void OnConnectionEstablished(object sender, RpcConnectionEventArgs e)
        {
            _logger.LogInformation("Connected to RPC server at {Endpoint}", e.RemoteEndPoint);
            _isConnected = true;
        }

        private void OnConnectionClosed(object sender, RpcConnectionEventArgs e)
        {
            _logger.LogWarning("Connection to RPC server at {Endpoint} closed", e.RemoteEndPoint);
            _isConnected = false;
            
            // TODO: Implement reconnection logic if needed
        }

        internal async Task<Protocol.RpcResponse> SendRequestAsync(Protocol.RpcRequest request)
        {
            EnsureConnected();
            

            var tcs = new TaskCompletionSource<Protocol.RpcResponse>();
            if (!_pendingRequests.TryAdd(request.MessageId, tcs))
            {
                throw new InvalidOperationException($"Duplicate request ID: {request.MessageId}");
            }

            try
            {
                // Serialize and send the request
                var messageSerializer = _serviceProvider.GetRequiredService<Protocol.RpcMessageSerializer>();
                var data = messageSerializer.SerializeMessage(request);
                await _transport.SendAsync(_serverEndpoint, data, CancellationToken.None);

                // Wait for response with timeout
                using (var cts = new CancellationTokenSource(request.TimeoutMs))
                {
                    var response = await tcs.Task.WaitAsync(cts.Token);
                    return response;
                }
            }
            catch (OperationCanceledException)
            {
                _pendingRequests.TryRemove(request.MessageId, out _);
                _logger.LogError("Request {MessageId} timed out after {TimeoutMs}ms", request.MessageId, request.TimeoutMs);
                throw new TimeoutException($"RPC request {request.MessageId} timed out after {request.TimeoutMs}ms");
            }
            catch (Exception ex)
            {
                _pendingRequests.TryRemove(request.MessageId, out _);
                _logger.LogError(ex, "Error in SendRequestAsync for request {MessageId}", request.MessageId);
                throw;
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// RPC client interface.
    /// </summary>
    public interface IRpcClient : IClusterClient
    {
    }
}
