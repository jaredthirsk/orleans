#nullable enable
using Forkleans.Configuration;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Forkleans.Configuration.Internal;
using Forkleans.Configuration.Validators;
using Forkleans.GrainReferences;
using Forkleans.Messaging;
using Forkleans.Metadata;
using Forkleans.Networking.Shared;
using Forkleans.Providers;
using Forkleans.Runtime;
using Forkleans.Runtime.Messaging;
using Forkleans.Runtime.Versions;
using Forkleans.Serialization;
using Forkleans.Statistics;
using Forkleans.Serialization.Serializers;
using Forkleans.Serialization.Cloning;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Forkleans.Serialization.Internal;
using System;
using Forkleans.Hosting;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Forkleans.Placement.Repartitioning;

namespace Forkleans
{
    /// <summary>
    /// Configures the default services for a client.
    /// </summary>
    internal static class DefaultClientServices
    {
        private static readonly ServiceDescriptor ServiceDescriptor = new(typeof(ServicesAdded), new ServicesAdded());

        /// <summary>
        /// Configures the default services for a client.
        /// </summary>
        /// <param name="builder">The client builder.</param>
        public static void AddDefaultServices(IClientBuilder builder)
        {
            var services = builder.Services;
            if (services.Contains(ServiceDescriptor))
            {
                return;
            }

            services.Add(ServiceDescriptor);

            // Common services
            services.AddLogging();
            services.AddOptions();
            services.TryAddSingleton<TimeProvider>(TimeProvider.System);

            // Options logging
            services.TryAddSingleton(typeof(IOptionFormatter<>), typeof(DefaultOptionsFormatter<>));
            services.TryAddSingleton(typeof(IOptionFormatterResolver<>), typeof(DefaultOptionsFormatterResolver<>));

            services.AddSingleton<ClientOptionsLogger>();
            services.AddFromExisting<ILifecycleParticipant<IClusterClientLifecycle>, ClientOptionsLogger>();

            // Statistics
            services.AddSingleton<IEnvironmentStatisticsProvider, EnvironmentStatisticsProvider>();
#pragma warning disable 618
            services.AddSingleton<OldEnvironmentStatistics>();
            services.AddFromExisting<IAppEnvironmentStatistics, OldEnvironmentStatistics>();
            services.AddFromExisting<IHostEnvironmentStatistics, OldEnvironmentStatistics>();
#pragma warning restore 618

            services.TryAddSingleton<GrainBindingsResolver>();
            services.TryAddSingleton<LocalClientDetails>();
            services.TryAddSingleton<OutsideRuntimeClient>();
            services.TryAddSingleton<InterfaceToImplementationMappingCache>();
            services.TryAddSingleton<ClientGrainContext>();
            services.AddSingleton<IClusterConnectionStatusObserver, ClusterConnectionStatusObserverAdaptor>();
            services.AddFromExisting<IGrainContextAccessor, ClientGrainContext>();
            services.TryAddFromExisting<IRuntimeClient, OutsideRuntimeClient>();
            services.TryAddFromExisting<IClusterConnectionStatusListener, OutsideRuntimeClient>();
            services.TryAddSingleton<GrainFactory>();
            services.TryAddSingleton<GrainInterfaceTypeToGrainTypeResolver>();
            services.TryAddSingleton<GrainReferenceActivator>();
            services.AddSingleton<IGrainReferenceActivatorProvider, GrainReferenceActivatorProvider>();
            services.AddSingleton<IGrainReferenceActivatorProvider, UntypedGrainReferenceActivatorProvider>();
            services.TryAddSingleton<RpcProvider>();
            services.TryAddSingleton<IGrainReferenceRuntime, GrainReferenceRuntime>();
            services.TryAddSingleton<GrainPropertiesResolver>();
            services.TryAddSingleton<IGrainCancellationTokenRuntime, GrainCancellationTokenRuntime>();
            services.TryAddFromExisting<IGrainFactory, GrainFactory>();
            services.TryAddFromExisting<IInternalGrainFactory, GrainFactory>();
            services.TryAddSingleton<ClientProviderRuntime>();
            services.TryAddSingleton<MessageFactory>();
            services.TryAddFromExisting<IProviderRuntime, ClientProviderRuntime>();
            services.TryAddSingleton<ClusterClient>();
            services.TryAddFromExisting<IClusterClient, ClusterClient>();
            services.TryAddFromExisting<IInternalClusterClient, ClusterClient>();
            services.AddFromExisting<IHostedService, ClusterClient>();
            services.AddTransient<IOptions<MessagingOptions>>(static sp => sp.GetRequiredService<IOptions<ClientMessagingOptions>>());
            services.TryAddSingleton<IClientConnectionRetryFilter, LinearBackoffClientConnectionRetryFilter>();

            services.AddSingleton<IConfigureOptions<GrainTypeOptions>, DefaultGrainTypeOptionsProvider>();

            // Add default option formatter if none is configured, for options which are required to be configured
            services.ConfigureFormatter<ClusterOptions>();
            services.ConfigureFormatter<ClientMessagingOptions>();
            services.ConfigureFormatter<ConnectionOptions>();

            services.AddTransient<IConfigurationValidator, GrainTypeOptionsValidator>();
            services.AddTransient<IConfigurationValidator, ClusterOptionsValidator>();
            services.AddTransient<IConfigurationValidator, ClientClusteringValidator>();
            services.AddTransient<IConfigurationValidator, SerializerConfigurationValidator>();

            // TODO: abstract or move into some options.
            services.AddSingleton<SocketSchedulers>();
            services.AddSingleton<SharedMemoryPool>();

            // Networking
            services.TryAddSingleton<IMessageStatisticsSink, NoOpMessageStatisticsSink>();
            services.TryAddSingleton<ConnectionCommon>();
            services.TryAddSingleton<ConnectionManager>();
            services.TryAddSingleton<ConnectionPreambleHelper>();
            services.AddSingleton<ILifecycleParticipant<IClusterClientLifecycle>, ConnectionManagerLifecycleAdapter<IClusterClientLifecycle>>();

            services.AddKeyedSingleton<IConnectionFactory>(
                ClientOutboundConnectionFactory.ServicesKey,
                (sp, key) => ActivatorUtilities.CreateInstance<SocketConnectionFactory>(sp));

            services.AddSerializer();
            services.AddSingleton<ITypeNameFilter, AllowOrleansTypes>();
            services.AddSingleton<ISpecializableCodec, GrainReferenceCodecProvider>();
            services.AddSingleton<ISpecializableCopier, GrainReferenceCopierProvider>();
            services.AddSingleton<OnDeserializedCallbacks>();
            services.AddSingleton<MigrationContext.SerializationHooks>();
            services.AddSingleton<IPostConfigureOptions<ForkleansJsonSerializerOptions>, ConfigureOrleansJsonSerializerOptions>();
            services.AddSingleton<ForkleansJsonSerializer>();

            services.TryAddTransient(sp => ActivatorUtilities.CreateInstance<MessageSerializer>(
                sp,
                sp.GetRequiredService<IOptions<ClientMessagingOptions>>().Value));
            services.TryAddSingleton<ConnectionFactory, ClientOutboundConnectionFactory>();
            services.TryAddSingleton<ClientMessageCenter>(sp => sp.GetRequiredService<OutsideRuntimeClient>().MessageCenter);
            services.TryAddFromExisting<IMessageCenter, ClientMessageCenter>();
            services.AddSingleton<GatewayManager>();
            services.AddSingleton<NetworkingTrace>();
            services.AddSingleton<MessagingTrace>();

            // Type metadata
            services.AddSingleton<ClientClusterManifestProvider>();
            services.AddFromExisting<IClusterManifestProvider, ClientClusterManifestProvider>();
            services.AddSingleton<ClientManifestProvider>();
            services.AddSingleton<IGrainInterfaceTypeProvider, AttributeGrainInterfaceTypeProvider>();
            services.AddSingleton<GrainTypeResolver>();
            services.AddSingleton<IGrainTypeProvider, AttributeGrainTypeProvider>();
            services.AddSingleton<GrainPropertiesResolver>();
            services.AddSingleton<GrainVersionManifest>();
            services.AddSingleton<GrainInterfaceTypeResolver>();
            services.AddSingleton<IGrainInterfacePropertiesProvider, AttributeGrainInterfacePropertiesProvider>();
            services.AddSingleton<IGrainPropertiesProvider, AttributeGrainPropertiesProvider>();
            services.AddSingleton<IGrainInterfacePropertiesProvider, TypeNameGrainPropertiesProvider>();
            services.AddSingleton<IGrainPropertiesProvider, TypeNameGrainPropertiesProvider>();
            services.AddSingleton<IGrainPropertiesProvider, ImplementedInterfaceProvider>();

            services.AddSingleton<IGrainCallCancellationManager, ExternalClientGrainCallCancellationManager>();

            ApplyConfiguration(builder);
        }

        private static void ApplyConfiguration(IClientBuilder builder)
        {
            var services = builder.Services;
            var cfg = builder.Configuration.GetSection("Forkleans");
            var knownProviderTypes = GetRegisteredProviders();

            services.Configure<ClusterOptions>(cfg);
            services.Configure<ClientMessagingOptions>(cfg.GetSection("Messaging"));
            services.Configure<GatewayOptions>(cfg.GetSection("Gateway"));

            if (bool.TryParse(cfg["EnableDistributedTracing"], out var enableDistributedTracing) && enableDistributedTracing)
            {
                builder.AddActivityPropagation();
            }

            ApplySubsection(builder, cfg, knownProviderTypes, "Clustering");
            ApplySubsection(builder, cfg, knownProviderTypes, "Reminders");
            ApplyNamedSubsections(builder, cfg, knownProviderTypes, "BroadcastChannel");
            ApplyNamedSubsections(builder, cfg, knownProviderTypes, "Streaming");
            ApplyNamedSubsections(builder, cfg, knownProviderTypes, "GrainStorage");
            ApplyNamedSubsections(builder, cfg, knownProviderTypes, "GrainDirectory");

            static void ConfigureProvider(
                IClientBuilder builder,
                Dictionary<(string Kind, string Name), Type> knownProviderTypes,
                string kind,
                string? name,
                IConfigurationSection configurationSection)
            {
                var providerType = configurationSection["ProviderType"] ?? "Default";
                var provider = GetRequiredProvider(knownProviderTypes, kind, providerType);
                provider.Configure(builder, name, configurationSection);
            }

            static IProviderBuilder<IClientBuilder> GetRequiredProvider(Dictionary<(string Kind, string Name), Type> knownProviderTypes, string kind, string name)
            {
                if (knownProviderTypes.TryGetValue((kind, name), out var type))
                {
                    var instance = Activator.CreateInstance(type);
                    return instance as IProviderBuilder<IClientBuilder>
                        ?? throw new InvalidOperationException($"{kind} provider, '{name}', of type {type}, does not implement {typeof(IProviderBuilder<IClientBuilder>)}.");
                }

                throw new InvalidOperationException($"Could not find {kind} provider named '{name}'. This can indicate that either the 'Microsoft.Forkleans.Sdk' package the provider's package are not referenced by your application.");
            }

            static Dictionary<(string Kind, string Name), Type> GetRegisteredProviders()
            {
                var result = new Dictionary<(string, string), Type>();
                foreach (var asm in ReferencedAssemblyProvider.GetRelevantAssemblies())
                {
                    foreach (var attr in asm.GetCustomAttributes<RegisterProviderAttribute>())
                    {
                        if (string.Equals(attr.Target, "Client"))
                        {
                            result[(attr.Kind, attr.Name)] = attr.Type;
                        }
                    }
                }

                return result;
            }

            static void ApplySubsection(IClientBuilder builder, IConfigurationSection cfg, Dictionary<(string Kind, string Name), Type> knownProviderTypes, string sectionName)
            {
                if (cfg.GetSection(sectionName) is { } section && section.Exists())
                {
                    ConfigureProvider(builder, knownProviderTypes, sectionName, name: null, section);
                }
            }

            static void ApplyNamedSubsections(IClientBuilder builder, IConfigurationSection cfg, Dictionary<(string Kind, string Name), Type> knownProviderTypes, string sectionName)
            {
                if (cfg.GetSection(sectionName) is { } section && section.Exists())
                {
                    foreach (var child in section.GetChildren())
                    {
                        ConfigureProvider(builder, knownProviderTypes, sectionName, name: child.Key, child);
                    }
                }
            }
        }

        internal partial class RootConfiguration
        {
            public IConfigurationSection? Clustering { get; set; }
        }

        /// <summary>
        /// A <see cref="ITypeNameFilter"/> which allows any type from an assembly containing "Forkleans" in its name to be allowed for the purposes of serialization and deserialization.
        /// </summary>
        private class AllowOrleansTypes : ITypeNameFilter
        {
            /// <inheritdoc />
            public bool? IsTypeNameAllowed(string typeName, string assemblyName)
            {
                if (assemblyName is { Length: > 0} && assemblyName.Contains("Forkleans"))
                {
                    return true;
                }

                return null;
            }
        }

        /// <summary>
        /// A marker type used to determine
        /// </summary>
        private class ServicesAdded { }
    }
}
