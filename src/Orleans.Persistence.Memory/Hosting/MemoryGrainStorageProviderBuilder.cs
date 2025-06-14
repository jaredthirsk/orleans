using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Forkleans;
using Forkleans.Configuration;
using Forkleans.Hosting;
using Forkleans.Providers;
using Forkleans.Runtime.Hosting.ProviderConfiguration;
using Forkleans.Storage;

[assembly: RegisterProvider("Memory", "GrainStorage", "Silo", typeof(MemoryGrainStorageProviderBuilder))]

namespace Forkleans.Runtime.Hosting.ProviderConfiguration;

internal sealed class MemoryGrainStorageProviderBuilder : IProviderBuilder<ISiloBuilder>
{
    public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
    {
        builder.AddMemoryGrainStorage(name, (OptionsBuilder<MemoryGrainStorageOptions> optionsBuilder) => optionsBuilder.Configure<IServiceProvider>((options, services) =>
        {
            if (int.TryParse(configurationSection[nameof(options.NumStorageGrains)], out var nsg))
            {
                options.NumStorageGrains = nsg;
            }

            var serializerKey = configurationSection["SerializerKey"];
            if (!string.IsNullOrEmpty(serializerKey))
            {
                options.GrainStorageSerializer = services.GetRequiredKeyedService<IGrainStorageSerializer>(serializerKey);
            }
        }));
    }
}
