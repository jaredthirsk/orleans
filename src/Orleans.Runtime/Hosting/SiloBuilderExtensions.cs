using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Forkleans.Hosting
{
    /// <summary>
    /// Extensions for <see cref="ISiloBuilder"/> instances.
    /// </summary>
    public static class SiloBuilderExtensions
    {
        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="builder">The <see cref="ISiloBuilder" /> to configure.</param>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="ISiloBuilder"/> for chaining.</returns>
        public static ISiloBuilder ConfigureServices(this ISiloBuilder builder, Action<IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            configureDelegate(builder.Services);
            return builder;
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="builder">The silo builder.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The silo builder.</returns>
        public static ISiloBuilder Configure<TOptions>(this ISiloBuilder builder, Action<TOptions> configureOptions) where TOptions : class
        {
            return builder.ConfigureServices(services => services.Configure(configureOptions));
        }

        /// <summary>
        /// Registers a configuration instance which <typeparamref name="TOptions"/> will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="builder">The silo builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The silo builder.</returns>
        public static ISiloBuilder Configure<TOptions>(this ISiloBuilder builder, IConfiguration configuration) where TOptions : class
        {
            return builder.ConfigureServices(services => services.AddOptions<TOptions>().Bind(configuration));
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggingBuilder"/>. This may be called multiple times.
        /// </summary>
        /// <param name="builder">The <see cref="ISiloBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The same instance of the <see cref="ISiloBuilder"/> for chaining.</returns>
        public static ISiloBuilder ConfigureLogging(this ISiloBuilder builder, Action<ILoggingBuilder> configureLogging)
        {
            return builder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }
    }
}