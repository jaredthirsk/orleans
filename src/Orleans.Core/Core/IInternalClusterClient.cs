namespace Forkleans
{
    /// <summary>
    /// The internal-facing client interface.
    /// </summary>
    internal interface IInternalClusterClient : IClusterClient, IInternalGrainFactory
    {
    }
}