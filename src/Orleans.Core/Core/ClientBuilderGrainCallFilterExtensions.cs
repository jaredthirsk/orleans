namespace Forkleans.Hosting;

/// <summary>
/// Extensions for configuring grain call filters.
/// </summary>
public static class ClientBuilderGrainCallFilterExtensions
{
    /// <summary>
    /// Adds an <see cref="IIncomingGrainCallFilter"/> to the filter pipeline.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>The builder.</returns>
    public static IClientBuilder AddIncomingGrainCallFilter(this IClientBuilder builder, IIncomingGrainCallFilter filter)
    {
        return builder.ConfigureServices(services => services.AddIncomingGrainCallFilter(filter));
    }

    /// <summary>
    /// Adds an <see cref="IIncomingGrainCallFilter"/> to the filter pipeline.
    /// </summary>
    /// <typeparam name="TImplementation">The filter implementation type.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IClientBuilder AddIncomingGrainCallFilter<TImplementation>(this IClientBuilder builder)
        where TImplementation : class, IIncomingGrainCallFilter
    {
        return builder.ConfigureServices(services => services.AddIncomingGrainCallFilter<TImplementation>());
    }

    /// <summary>
    /// Adds an <see cref="IIncomingGrainCallFilter"/> to the filter pipeline via a delegate.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>The builder.</returns>
    public static IClientBuilder AddIncomingGrainCallFilter(this IClientBuilder builder, IncomingGrainCallFilterDelegate filter)
    {
        return builder.ConfigureServices(services => services.AddIncomingGrainCallFilter(filter));
    }

    /// <summary>
    /// Adds an <see cref="IOutgoingGrainCallFilter"/> to the filter pipeline.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>The <see cref="IClientBuilder"/>.</returns>
    public static IClientBuilder AddOutgoingGrainCallFilter(this IClientBuilder builder, IOutgoingGrainCallFilter filter)
    {
        return builder.ConfigureServices(services => services.AddOutgoingGrainCallFilter(filter));
    }

    /// <summary>
    /// Adds an <see cref="IOutgoingGrainCallFilter"/> to the filter pipeline.
    /// </summary>
    /// <typeparam name="TImplementation">The filter implementation type.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The <see cref="IClientBuilder"/>.</returns>
    public static IClientBuilder AddOutgoingGrainCallFilter<TImplementation>(this IClientBuilder builder)
        where TImplementation : class, IOutgoingGrainCallFilter
    {
        return builder.ConfigureServices(services => services.AddOutgoingGrainCallFilter<TImplementation>());
    }

    /// <summary>
    /// Adds an <see cref="IOutgoingGrainCallFilter"/> to the filter pipeline via a delegate.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>The <see cref="IClientBuilder"/>.</returns>
    public static IClientBuilder AddOutgoingGrainCallFilter(this IClientBuilder builder, OutgoingGrainCallFilterDelegate filter)
    {
        return builder.ConfigureServices(services => services.AddOutgoingGrainCallFilter(filter));
    }
}