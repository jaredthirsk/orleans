using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Forkleans.Providers.Streams.PersistentStreams;
using Forkleans.Serialization;
using Forkleans.Streaming.EventHubs;
using Forkleans.Streams;
using TestExtensions;
using Tester.AzureUtils;

namespace ServiceBus.Tests.TestStreamProviders.EventHub
{
    public class TestAzureTableStorageStreamFailureHandler : AzureTableStorageStreamFailureHandler<StreamDeliveryFailureEntity>
    {
        private const string TableName = "TestStreamFailures";
        private const string DeploymentId = "TestDeployment";
        private TestAzureTableStorageStreamFailureHandler(Serializer<StreamSequenceToken> serializer)
            : base(serializer, NullLoggerFactory.Instance, false, DeploymentId, GetStreamingAzureStorageOperationOptions())
        {
        }

        public static async Task<IStreamFailureHandler> Create(Serializer<StreamSequenceToken> serializer)
        {
            var failureHandler = new TestAzureTableStorageStreamFailureHandler(serializer);
            await failureHandler.InitAsync();
            return failureHandler;
        }

        public static async Task<int> GetDeliveryFailureCount(string streamProviderName)
        {
            var dataManager = GetDataManager();
            await dataManager.InitTableAsync();
            var deliveryErrors =
                await dataManager.ReadAllTableEntriesForPartitionAsync(
                        StreamDeliveryFailureEntity.MakeDefaultPartitionKey(streamProviderName, DeploymentId));
            return deliveryErrors.Count;
        }

        private static AzureTableDataManager<TableEntity> GetDataManager()
        {
            var options = GetAzureStorageOperationOptions();
            return new AzureTableDataManager<TableEntity>(options, NullLogger.Instance);
        }

        private static AzureStorageOperationOptions GetAzureStorageOperationOptions()
        {
            var options = new AzureStorageOperationOptions { TableName = TableName };
            options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient();

            return options;
        }

        private static Forkleans.Streaming.AzureStorage.AzureStorageOperationOptions GetStreamingAzureStorageOperationOptions()
        {
            var options = new Forkleans.Streaming.AzureStorage.AzureStorageOperationOptions { TableName = TableName };
            options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient();

            return options;
        }
    }
}
