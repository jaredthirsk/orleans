using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Forkleans.Configuration;
using Forkleans.Providers.Streams.Generator;
using Forkleans.Runtime;
using Forkleans.Streams;
using Forkleans.TestingHost;
using Forkleans.TestingHost.Utils;
using TestExtensions;
using TestGrainInterfaces;
using TestGrains;
using UnitTests.Grains;
using Xunit;

namespace UnitTests.StreamingTests
{
    public class StreamGeneratorProviderTests : OrleansTestingBase, IClassFixture<StreamGeneratorProviderTests.Fixture>
    {
        private const int TotalQueueCount = 4;
        private readonly Fixture fixture;

        public class Fixture : BaseTestClusterFixture
        {
            public const string StreamProviderName = GeneratedStreamTestConstants.StreamProviderName;
            public const string StreamNamespace = GeneratedEventCollectorGrain.StreamNamespace;

            public readonly static SimpleGeneratorOptions GeneratorConfig = new SimpleGeneratorOptions
            {
                StreamNamespace = StreamNamespace,
                EventsInStream = 100
            };

            protected override void ConfigureTestCluster(TestClusterBuilder builder)
            {
                builder.AddSiloBuilderConfigurator<MySiloBuilderConfigurator>();
            }

            private class MySiloBuilderConfigurator : ISiloConfigurator
            {
                public void Configure(ISiloBuilder hostBuilder)
                {
                    hostBuilder
                        .ConfigureLogging(logging => logging.AddDebug())
                        .ConfigureServices(services => services.AddKeyedSingleton<IStreamGeneratorConfig>(StreamProviderName, (s, n) => GeneratorConfig))
                        .AddPersistentStreams(
                            StreamProviderName,
                            GeneratorAdapterFactory.Create,
                            b =>
                            {
                                b.ConfigurePullingAgent(ob => ob.Configure(options => { options.BatchContainerBatchSize = 10; }));
                                b.Configure<HashRingStreamQueueMapperOptions>(ob => ob.Configure(options => options.TotalQueueCount = TotalQueueCount));
                                b.UseDynamicClusterConfigDeploymentBalancer();
                                b.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
                            });

                }
            }
        }

        public StreamGeneratorProviderTests(Fixture fixture)
        {
            this.fixture = fixture;
        }

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        [Fact, TestCategory("BVT"), TestCategory("Streaming")]
        public async Task ValidateGeneratedStreamsTest()
        {
            this.fixture.Logger.LogInformation("************************ ValidateGeneratedStreamsTest *********************************");
            await TestingUtils.WaitUntilAsync(CheckCounters, Timeout);
        }

        private async Task<bool> CheckCounters(bool assertIsTrue)
        {
            var reporter = this.fixture.GrainFactory.GetGrain<IGeneratedEventReporterGrain>(GeneratedStreamTestConstants.ReporterId);

            var report = await reporter.GetReport(Fixture.StreamProviderName, Fixture.StreamNamespace);
            if (assertIsTrue)
            {
                // one stream per queue
                Assert.Equal(TotalQueueCount, report.Count);
                foreach (int eventsPerStream in report.Values)
                {
                    Assert.Equal(Fixture.GeneratorConfig.EventsInStream, eventsPerStream);
                }
            }
            else if (TotalQueueCount != report.Count ||
                     report.Values.Any(count => count != Fixture.GeneratorConfig.EventsInStream))
            {
                return false;
            }
            return true;
        }
    }
}
