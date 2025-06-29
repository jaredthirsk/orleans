using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Forkleans;
using Forkleans.Configuration;
using Forkleans.Configuration.Internal;
using Forkleans.Configuration.Validators;
using Forkleans.Hosting;
using Forkleans.Runtime;
using Forkleans.Statistics;
using UnitTests.Grains;
using Xunit;

namespace NonSilo.Tests
{
    public class NoOpMembershipTable : IMembershipTable
    {
        public Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            return Task.CompletedTask;
        }

        public Task DeleteMembershipTableEntries(string clusterId)
        {
            return Task.CompletedTask;
        }

        public Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            return Task.CompletedTask;
        }

        public Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            return Task.FromResult(true);
        }

        public Task<MembershipTableData> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            throw new NotImplementedException();
        }

        public Task UpdateIAmAlive(MembershipEntry entry)
        {
            return Task.CompletedTask;
        }

        public Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Tests for <see cref="ISiloBuilder"/>.
    /// </summary>
    [TestCategory("BVT")]
    [TestCategory("Hosting")]
    public class SiloBuilderTests
    {
        [Fact]
        public void SiloBuilderTest()
        {
            var host = new HostBuilder()
                .UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options => options.ClusterId = "someClusterId")
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback);
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                })
                .Build();

            var clusterClient = host.Services.GetRequiredService<IClusterClient>();
        }

        /// <summary>
        /// Grain's CollectionAgeLimit must be > 0 minutes.
        /// </summary>
        [Fact]
        public async Task SiloBuilder_GrainCollectionOptionsForZeroSecondsAgeLimitTest()
        {
            await Assert.ThrowsAsync<ForkleansConfigurationException>(async () =>
            {
                await new HostBuilder().UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                        .Configure<ClusterOptions>(options => { options.ClusterId = "GrainCollectionClusterId"; options.ServiceId = "GrainCollectionServiceId"; })
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureServices(services => services.AddSingleton<IMembershipTable, NoOpMembershipTable>())
                        .Configure<GrainCollectionOptions>(options => options
                                    .ClassSpecificCollectionAge
                                    .Add(typeof(CollectionSpecificAgeLimitForZeroSecondsActivationGcTestGrain).FullName, TimeSpan.Zero));
                }).RunConsoleAsync();
            });
        }

        /// <summary>
        /// ClusterMembershipOptions.NumProbedSilos must be greater than ClusterMembershipOptions.NumVotesForDeathDeclaration.
        /// </summary>
        [Fact]
        public async Task SiloBuilder_ClusterMembershipOptionsValidators()
        {
            await Assert.ThrowsAsync<ForkleansConfigurationException>(async () =>
            {
                await new HostBuilder().UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterMembershipOptions>(options => { options.NumVotesForDeathDeclaration = 10; options.NumProbedSilos = 1; });
                }).RunConsoleAsync();
            });

            await Assert.ThrowsAsync<ForkleansConfigurationException>(async () =>
            {
                await new HostBuilder().UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterMembershipOptions>(options => { options.NumVotesForDeathDeclaration = 0; });
                }).RunConsoleAsync();
            });
        }

        /// <summary>
        /// Ensures <see cref="LoadSheddingValidator"/> fails when LoadSheddingLimit greater than 100.
        /// </summary>
        [Fact]
        public async Task SiloBuilder_LoadSheddingValidatorAbove100ShouldFail()
        {
            await Assert.ThrowsAsync<ForkleansConfigurationException>(async () =>
            {
                await new HostBuilder().UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options => options.ClusterId = "someClusterId")
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureServices(services => services.AddSingleton<IMembershipTable, NoOpMembershipTable>())
                        .Configure<LoadSheddingOptions>(options =>
                        {
                            options.LoadSheddingEnabled = true;
                            options.CpuThreshold = 101;
                        })
                        .ConfigureServices(svcCollection =>
                        {
                            svcCollection.AddSingleton<FakeEnvironmentStatisticsProvider>();
                            svcCollection.AddFromExisting<IEnvironmentStatisticsProvider, FakeEnvironmentStatisticsProvider>();
                            svcCollection.AddTransient<IConfigurationValidator, LoadSheddingValidator>();
                        });
                }).RunConsoleAsync();
            });
        }

        [Fact]
        public async Task SiloBuilderThrowsDuringStartupIfNoGrainsAdded()
        {
            using var host = new HostBuilder()
                .UseOrleans((ctx, siloBuilder) =>
                {
                    // Add only an assembly with generated serializers but no grain interfaces or grain classes
                    siloBuilder.UseLocalhostClustering()
                    .Configure<GrainTypeOptions>(options =>
                    {
                        options.Classes.Clear();
                        options.Interfaces.Clear();
                    });
                }).Build();

            await Assert.ThrowsAsync<ForkleansConfigurationException>(() => host.StartAsync());
        }

        [Fact]
        public void SiloBuilderThrowsDuringStartupIfClientBuildersAdded()
        {
            Assert.Throws<ForkleansConfigurationException>(() =>
            {
                _ = new HostBuilder()
                    .UseOrleansClient((ctx, clientBuilder) =>
                    {
                        clientBuilder.UseLocalhostClustering();
                    })
                    .UseOrleans((ctx, siloBuilder) =>
                    {
                        siloBuilder.UseLocalhostClustering();
                    });
            });
        }

        [Fact]
        public void SiloBuilderWithHotApplicationBuilderThrowsDuringStartupIfClientBuildersAdded()
        {
            Assert.Throws<ForkleansConfigurationException>(() =>
            {
                _ = Host.CreateApplicationBuilder()
                    .UseOrleansClient(clientBuilder =>
                    {
                        clientBuilder.UseLocalhostClustering();
                    })
                    .UseOrleans(siloBuilder =>
                    {
                        siloBuilder.UseLocalhostClustering();
                    });
            });
        }

        private class FakeEnvironmentStatisticsProvider : IEnvironmentStatisticsProvider
        {
            public EnvironmentStatistics GetEnvironmentStatistics() => new();
        }

        private class MyService
        {
            public int Id { get; set; }
        }
    }
}