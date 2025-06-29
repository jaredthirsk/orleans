using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Forkleans.AzureUtils;
using Forkleans.Clustering.AzureStorage;
using Forkleans.Configuration;
using Forkleans.Messaging;
using Forkleans.Runtime.MembershipService;

namespace Forkleans.Hosting
{
    public static class AzureTableClusteringExtensions
    {
        /// <summary>
        /// Configures the silo to use Azure Storage for clustering.
        /// </summary>
        /// <param name="builder">
        /// The silo builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="ISiloBuilder"/>.
        /// </returns>
        public static ISiloBuilder UseAzureStorageClustering(
            this ISiloBuilder builder,
            Action<AzureStorageClusteringOptions> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    if (configureOptions != null)
                    {
                        services.Configure(configureOptions);
                    }

                    services.AddSingleton<IMembershipTable, AzureBasedMembershipTable>()
                    .ConfigureFormatter<AzureStorageClusteringOptions>();
                });
        }

        /// <summary>
        /// Configures the silo to use Azure Storage for clustering.
        /// </summary>
        /// <param name="builder">
        /// The silo builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="ISiloBuilder"/>.
        /// </returns>
        public static ISiloBuilder UseAzureStorageClustering(
            this ISiloBuilder builder,
            Action<OptionsBuilder<AzureStorageClusteringOptions>> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    configureOptions?.Invoke(services.AddOptions<AzureStorageClusteringOptions>());
                    services.AddTransient<IConfigurationValidator>(sp => new AzureStorageClusteringOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>().Get(Options.DefaultName), Options.DefaultName));
                    services.AddSingleton<IMembershipTable, AzureBasedMembershipTable>()
                    .ConfigureFormatter<AzureStorageClusteringOptions>();
                });
        }

        /// <summary>
        /// Configures the client to use Azure Storage for clustering.
        /// </summary>
        /// <param name="builder">
        /// The client builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="IClientBuilder"/>.
        /// </returns>
        public static IClientBuilder UseAzureStorageClustering(
            this IClientBuilder builder,
            Action<AzureStorageGatewayOptions> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    if (configureOptions != null)
                    {
                        services.Configure(configureOptions);
                    }

                    services.AddSingleton<IGatewayListProvider, AzureGatewayListProvider>()
                    .ConfigureFormatter<AzureStorageGatewayOptions>();
                });
        }

        /// <summary>
        /// Configures the client to use Azure Storage for clustering.
        /// </summary>
        /// <param name="builder">
        /// The client builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="IClientBuilder"/>.
        /// </returns>
        public static IClientBuilder UseAzureStorageClustering(
            this IClientBuilder builder,
            Action<OptionsBuilder<AzureStorageGatewayOptions>> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    configureOptions?.Invoke(services.AddOptions<AzureStorageGatewayOptions>());
                    services.AddTransient<IConfigurationValidator>(sp => new AzureStorageGatewayOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureStorageGatewayOptions>>().Get(Options.DefaultName), Options.DefaultName));
                    services.AddSingleton<IGatewayListProvider, AzureGatewayListProvider>()
                    .ConfigureFormatter<AzureStorageGatewayOptions>();
                });
        }
    }
}
