using System;
using Microsoft.Extensions.DependencyInjection;
using Forkleans.Runtime;
using Forkleans.Runtime.Placement;

namespace Forkleans.Hosting
{
    /// <summary>
    /// Extensions for configuring grain placement.
    /// </summary>
    public static class PlacementStrategyExtensions
    {
        /// <summary>
        /// Configures a <typeparamref name="TDirector"/> as the placement director for placement strategy <typeparamref name="TStrategy"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <typeparam name="TDirector">The placement director.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static ISiloBuilder AddPlacementDirector<TStrategy, TDirector>(this ISiloBuilder builder)
            where TStrategy : PlacementStrategy, new()
            where TDirector : class, IPlacementDirector
        {
            return builder.ConfigureServices(services => services.AddPlacementDirector<TStrategy, TDirector>());
        }

        /// <summary>
        /// Adds a placement director.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="createDirector">The delegate used to create the placement director.</param>
        /// <returns>The builder.</returns>
        public static ISiloBuilder AddPlacementDirector<TStrategy>(this ISiloBuilder builder, Func<IServiceProvider, IPlacementDirector> createDirector)
            where TStrategy : PlacementStrategy, new()
        {
            return builder.ConfigureServices(services => services.AddPlacementDirector<TStrategy>(createDirector));
        }

        /// <summary>
        /// Configures a <typeparamref name="TDirector"/> as the placement director for placement strategy <typeparamref name="TStrategy"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <typeparam name="TDirector">The placement director.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static void AddPlacementDirector<TStrategy, TDirector>(this IServiceCollection services)
            where TStrategy : PlacementStrategy, new()
            where TDirector : class, IPlacementDirector => services.AddPlacementDirector<TStrategy, TDirector>(ServiceLifetime.Singleton);

        /// <summary>
        /// Configures a <typeparamref name="TDirector"/> as the placement director for placement strategy <typeparamref name="TStrategy"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <typeparam name="TDirector">The placement director.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="strategyLifetime">The lifetime of the placement strategy.</param>
        /// <returns>The service collection.</returns>
        public static void AddPlacementDirector<TStrategy, TDirector>(this IServiceCollection services, ServiceLifetime strategyLifetime)
            where TStrategy : PlacementStrategy, new()
            where TDirector : class, IPlacementDirector
        {
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(PlacementStrategy), typeof(TStrategy).Name, typeof(TStrategy), strategyLifetime));
            services.AddKeyedSingleton<IPlacementDirector, TDirector>(typeof(TStrategy));
        }

        /// <summary>
        /// Adds a placement director.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="createDirector">The delegate used to create the placement director.</param>
        /// <returns>The service collection.</returns>
        public static void AddPlacementDirector<TStrategy>(this IServiceCollection services, Func<IServiceProvider, IPlacementDirector> createDirector)
            where TStrategy : PlacementStrategy, new() => services.AddPlacementDirector<TStrategy>(createDirector, ServiceLifetime.Singleton);

        /// <summary>
        /// Adds a placement director.
        /// </summary>
        /// <typeparam name="TStrategy">The placement strategy.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="createDirector">The delegate used to create the placement director.</param>
        /// <param name="strategyLifetime">The lifetime of the placement strategy.</param>
        /// <returns>The service collection.</returns>
        public static void AddPlacementDirector<TStrategy>(this IServiceCollection services, Func<IServiceProvider, IPlacementDirector> createDirector, ServiceLifetime strategyLifetime)
            where TStrategy : PlacementStrategy, new()
        {
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(PlacementStrategy), typeof(TStrategy).Name, typeof(TStrategy), strategyLifetime));
            services.AddKeyedSingleton<IPlacementDirector>(typeof(TStrategy), (sp, type) => createDirector(sp));
        }
    }
}