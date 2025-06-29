using System.Net;
using Forkleans.Messaging;
using Forkleans.Clustering.Cosmos.Models;

namespace Forkleans.Clustering.Cosmos;

internal partial class CosmosGatewayListProvider : IGatewayListProvider
{
    private readonly ILogger _logger;
    private readonly string _clusterId;
    private readonly IServiceProvider _serviceProvider;
    private readonly CosmosClusteringOptions _options;
    private readonly QueryRequestOptions _queryRequestOptions;
    private Container _container = default!;

    public TimeSpan MaxStaleness { get; }

    public bool IsUpdatable => true;

    public CosmosGatewayListProvider(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IOptions<CosmosClusteringOptions> options,
        IOptions<ClusterOptions> clusterOptions,
        IOptions<GatewayOptions> gatewayOptions
    )
    {
        _logger = loggerFactory.CreateLogger<CosmosGatewayListProvider>();
        _serviceProvider = serviceProvider;
        _clusterId = clusterOptions.Value.ClusterId;
        _options = options.Value;
        _queryRequestOptions = new QueryRequestOptions { PartitionKey = new(_clusterId) };
        MaxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
    }

    public async Task InitializeGatewayListProvider()
    {
        try
        {
            var client = await _options.CreateClient!(_serviceProvider).ConfigureAwait(false);
            _container = client.GetContainer(_options.DatabaseName, _options.ContainerName);
        }
        catch (Exception ex)
        {
            LogErrorInitializingGatewayListProvider(ex);
            throw;
        }
    }

    public async Task<IList<Uri>> GetGateways()
    {
        try
        {
            var query = _container
                .GetItemLinqQueryable<SiloEntity>(requestOptions: _queryRequestOptions)
                .Where(g => g.EntityType == nameof(SiloEntity) &&
                    g.Status == (int)SiloStatus.Active &&
                    g.ProxyPort.HasValue && g.ProxyPort.Value != 0)
                .ToFeedIterator();

            var entities = new List<SiloEntity>();
            do
            {
                var items = await query.ReadNextAsync();
                entities.AddRange(items);
            } while (query.HasMoreResults);

            var uris = entities.Select(ConvertToGatewayUri).ToArray();
            return uris;
        }
        catch (Exception ex)
        {
            LogErrorReadingGatewayListFromCosmosDb(ex);
            throw;
        }
    }

    private static Uri ConvertToGatewayUri(SiloEntity gateway) =>
        SiloAddress.New(new IPEndPoint(IPAddress.Parse(gateway.Address), gateway.ProxyPort!.Value), gateway.Generation).ToGatewayUri();

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error initializing Azure Cosmos DB gateway list provider"
    )]
    private partial void LogErrorInitializingGatewayListProvider(Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error reading gateway list from Azure Cosmos DB"
    )]
    private partial void LogErrorReadingGatewayListFromCosmosDb(Exception ex);
}
