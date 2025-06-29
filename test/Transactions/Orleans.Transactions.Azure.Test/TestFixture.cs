using Forkleans.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Forkleans.TestingHost;
using Forkleans.Transactions.TestKit;
using Forkleans.Transactions.Tests;
using TestExtensions;
using Tester;
using Microsoft.Extensions.Configuration;
using Tester.AzureUtils;

namespace Forkleans.Transactions.AzureStorage.Tests
{
    public class TestFixture : BaseTestClusterFixture
    {
        protected override void CheckPreconditionsOrThrow()
        {
            base.CheckPreconditionsOrThrow();
            TestUtils.CheckForAzureStorage();
        }

        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
            builder.AddClientBuilderConfigurator<ClientBuilderConfigurator>();
        }

        public class SiloBuilderConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder hostBuilder)
            {
                hostBuilder
                    .ConfigureServices(services => services.AddKeyedSingleton<IRemoteCommitService, RemoteCommitService>(TransactionTestConstants.RemoteCommitService))
                    .AddAzureTableTransactionalStateStorage(TransactionTestConstants.TransactionStore, options =>
                    {
                        options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient();
                    })
                    .UseTransactions();
            }
        }

        public class ClientBuilderConfigurator : IClientBuilderConfigurator
        {
            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                clientBuilder
                    .UseTransactions();
            }
        }
    }

    public class ControlledFaultInjectionTestFixture : BaseTestClusterFixture
    {
        protected override void CheckPreconditionsOrThrow()
        {
            base.CheckPreconditionsOrThrow();
            TestUtils.CheckForAzureStorage();
        }

        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
        }

        public class SiloBuilderConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder hostBuilder)
            {
                hostBuilder
                    .AddFaultInjectionAzureTableTransactionalStateStorage(TransactionTestConstants.TransactionStore, options =>
                    {
                        options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient();
                    })
                    .UseControlledFaultInjectionTransactionState()
                    .UseTransactions()
                    .ConfigureServices(svc =>
                    {
                        svc.AddScoped<ITransactionFaultInjector, SimpleAzureStorageExceptionInjector>()
                        .AddScoped<IControlledTransactionFaultInjector>(sp => sp.GetService<ITransactionFaultInjector>() as IControlledTransactionFaultInjector);
                    });
            }
        }
    }

    public class SkewedClockTestFixture : TestFixture
    {
        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            builder.AddSiloBuilderConfigurator<SkewedClockConfigurator>();
            base.ConfigureTestCluster(builder);
        }
    }


    public class RandomFaultInjectedTestFixture : TestFixture
    {
        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            builder.AddSiloBuilderConfigurator<TxSiloBuilderConfigurator>();
            base.ConfigureTestCluster(builder);
        }

        public class TxSiloBuilderConfigurator : ISiloConfigurator
        {
            private static readonly double probability = 0.05;
            public void Configure(ISiloBuilder hostBuilder)
            {
                hostBuilder
                    .AddFaultInjectionAzureTableTransactionalStateStorage(TransactionTestConstants.TransactionStore, options =>
                    {
                        options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient();
                    })
                    .UseTransactions()
                    .ConfigureServices(services => services.AddSingleton<ITransactionFaultInjector>(sp => new RandomErrorInjector(probability)));
            }
        }
    }

}
