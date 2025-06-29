using Microsoft.Extensions.DependencyInjection;
using Forkleans.Configuration;
using Forkleans.Providers;
using Forkleans.Storage;
using Forkleans.TestingHost;
using Microsoft.Extensions.Logging.Abstractions;
using TesterInternal;
using UnitTests.GrainInterfaces;
using Xunit;
using Xunit.Abstractions;
using static Forkleans.Storage.DynamoDBGrainStorage;
using TestExtensions.Runners;

namespace AWSUtils.Tests.StorageTests
{
    [TestCategory("Persistence"), TestCategory("AWS"), TestCategory("DynamoDb")]
    public class PersistenceGrainTests_AWSDynamoDBStore : GrainPersistenceTestsRunner, IClassFixture<PersistenceGrainTests_AWSDynamoDBStore.Fixture>
    {
        public class Fixture : TestExtensions.BaseTestClusterFixture
        {
            protected override void ConfigureTestCluster(TestClusterBuilder builder)
            {
                if (AWSTestConstants.IsDynamoDbAvailable)
                {
                    builder.Options.InitialSilosCount = 4;
                    builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
                }
            }

            public class SiloBuilderConfigurator : ISiloConfigurator
            {
                public void Configure(ISiloBuilder hostBuilder)
                {
                    hostBuilder.AddMemoryGrainStorage("MemoryStore");
                    hostBuilder.AddMemoryGrainStorage("test1");
                    hostBuilder.AddDynamoDBGrainStorage("GrainStorageForTest", options => options.Service = AWSTestConstants.DynamoDbService);
                }
            }
        }

        public PersistenceGrainTests_AWSDynamoDBStore(ITestOutputHelper output, Fixture fixture) : base(output, fixture, grainNamespace: "UnitTests.Grains")
        {
            if (!AWSTestConstants.IsDynamoDbAvailable)
            {
                output.WriteLine("Unable to connect to AWS DynamoDB simulator");
                throw new SkipException("Unable to connect to AWS DynamoDB simulator");
            }
        }

        [SkippableFact, TestCategory("Functional")]
        public async Task AWSDynamoDBStore_ConvertToFromStorageFormat_GrainReference()
        {
            // NOTE: This test requires Silo to be running & Client init so that grain references can be resolved before serialization.
            Guid id = Guid.NewGuid();
            IUser grain = this.HostedCluster.GrainFactory.GetGrain<IUser>(id);

            var initialState = new GrainStateContainingGrainReferences { Grain = grain };
            var entity = new GrainStateRecord();
            var storage = await InitDynamoDBTableStorageProvider(
                this.HostedCluster.ServiceProvider.GetRequiredService<IProviderRuntime>(), "TestTable");
            storage.ConvertToStorageFormat(initialState, entity);
            var convertedState = storage.ConvertFromStorageFormat<GrainStateContainingGrainReferences>(entity);
            Assert.NotNull(convertedState); // Converted state
            Assert.Equal(initialState.Grain, convertedState.Grain);  // "Grain"
        }

        [SkippableFact, TestCategory("Functional")]
        public async Task AWSDynamoDBStore_ConvertToFromStorageFormat_GrainReference_List()
        {
            // NOTE: This test requires Silo to be running & Client init so that grain references can be resolved before serialization.
            Guid[] ids = { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            IUser[] grains = new IUser[3];
            grains[0] = this.HostedCluster.GrainFactory.GetGrain<IUser>(ids[0]);
            grains[1] = this.HostedCluster.GrainFactory.GetGrain<IUser>(ids[1]);
            grains[2] = this.HostedCluster.GrainFactory.GetGrain<IUser>(ids[2]);

            var initialState = new GrainStateContainingGrainReferences();
            foreach (var g in grains)
            {
                initialState.GrainList.Add(g);
                initialState.GrainDict.Add(g.GetPrimaryKey().ToString(), g);
            }
            var entity = new GrainStateRecord();
            var storage =
                await InitDynamoDBTableStorageProvider(
                    this.HostedCluster.ServiceProvider.GetRequiredService<IProviderRuntime>(), "TestTable");
            storage.ConvertToStorageFormat(initialState, entity);
            var convertedState = storage.ConvertFromStorageFormat<GrainStateContainingGrainReferences>(entity);
            Assert.NotNull(convertedState);
            Assert.Equal(initialState.GrainList.Count, convertedState.GrainList.Count);  // "GrainList size"
            Assert.Equal(initialState.GrainDict.Count, convertedState.GrainDict.Count);  // "GrainDict size"
            for (int i = 0; i < grains.Length; i++)
            {
                string iStr = ids[i].ToString();
                Assert.Equal(initialState.GrainList[i], convertedState.GrainList[i]);  // "GrainList #{0}", i
                Assert.Equal(initialState.GrainDict[iStr], convertedState.GrainDict[iStr]);  // "GrainDict #{0}", i
            }
            Assert.Equal(initialState.Grain, convertedState.Grain);  // "Grain"
        }

        private static async Task<DynamoDBGrainStorage> InitDynamoDBTableStorageProvider(IProviderRuntime runtime, string storageName)
        {
            var options = new DynamoDBStorageOptions();
            options.Service = AWSTestConstants.DynamoDbService;
            options.GrainStorageSerializer = ActivatorUtilities.CreateInstance<ForkleansGrainStorageSerializer>(runtime.ServiceProvider);

            DynamoDBGrainStorage store = ActivatorUtilities.CreateInstance<DynamoDBGrainStorage>(runtime.ServiceProvider, "PersistenceGrainTests", options);
            ISiloLifecycleSubject lifecycle = ActivatorUtilities.CreateInstance<SiloLifecycleSubject>(runtime.ServiceProvider, NullLogger<SiloLifecycleSubject>.Instance);
            store.Participate(lifecycle);
            await lifecycle.OnStart();
            return store;
        }
    }
}
