using System;

namespace Forkleans.Runtime
{
    /// <summary>
    /// The silo role placement strategy specifies that a grain should be placed on a compatible silo which has the role specified by the strategy's placement attribute.
    /// </summary>
    [Serializable, GenerateSerializer, Immutable, SuppressReferenceTracking]
    public class SiloRoleBasedPlacement : PlacementStrategy
    {
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        internal static SiloRoleBasedPlacement Singleton { get; } = new SiloRoleBasedPlacement();
    }
}
