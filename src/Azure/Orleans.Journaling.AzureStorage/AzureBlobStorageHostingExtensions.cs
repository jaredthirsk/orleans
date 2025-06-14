using Microsoft.Extensions.DependencyInjection;
using Forkleans.Configuration.Internal;
using Forkleans.Runtime;
using Forkleans.Hosting;

namespace Forkleans.Journaling;

public static class AzureBlobStorageHostingExtensions
{
    public static ISiloBuilder AddAzureAppendBlobStateMachineStorage(this ISiloBuilder builder) => builder.AddAzureAppendBlobStateMachineStorage(configure: null);
    public static ISiloBuilder AddAzureAppendBlobStateMachineStorage(this ISiloBuilder builder, Action<AzureAppendBlobStateMachineStorageOptions>? configure)
    {
        builder.AddStateMachineStorage();

        var services = builder.Services;

        var options = builder.Services.AddOptions<AzureAppendBlobStateMachineStorageOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        if (services.Any(service => service.ServiceType.Equals(typeof(AzureAppendBlobStateMachineStorageProvider))))
        {
            return builder;
        }

        builder.Services.AddSingleton<AzureAppendBlobStateMachineStorageProvider>();
        builder.Services.AddFromExisting<IStateMachineStorageProvider, AzureAppendBlobStateMachineStorageProvider>();
        builder.Services.AddFromExisting<ILifecycleParticipant<ISiloLifecycle>, AzureAppendBlobStateMachineStorageProvider>();
        return builder;
    }
}
