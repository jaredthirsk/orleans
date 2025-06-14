using System.Collections.Generic;
using System.Collections.Immutable;

namespace Forkleans.Runtime
{
    /// <summary>
    /// Authoritative local, per-silo source for information about the status of other silos.
    /// </summary>
    public interface ISiloStatusOracle
    {
        /// <summary>
        /// Gets the current status of this silo.
        /// </summary>
        SiloStatus CurrentStatus { get; }

        /// <summary>
        /// Gets the name of this silo.
        /// </summary>
        string SiloName { get; }

        /// <summary>
        /// Gets the address of this silo.
        /// </summary>
        SiloAddress SiloAddress { get; }

        /// <summary>
        /// Gets the currently active silos.
        /// </summary>
        ImmutableArray<SiloAddress> GetActiveSilos();

        /// <summary>
        /// Gets the status of a given silo. 
        /// This method returns an approximate view on the status of a given silo. 
        /// In particular, this oracle may think the given silo is alive, while it may already have failed.
        /// If this oracle thinks the given silo is dead, it has been authoritatively told so by ISiloDirectory.
        /// </summary>
        /// <param name="siloAddress">A silo whose status we are interested in.</param>
        /// <returns>The status of a given silo.</returns>
        SiloStatus GetApproximateSiloStatus(SiloAddress siloAddress);

        /// <summary>
        /// Gets the statuses of all silo. 
        /// This method returns an approximate view on the statuses of all silo.
        /// </summary>
        /// <param name="onlyActive">Include only silo who are currently considered to be active. If false, include all.</param>
        /// <returns>A list of silo statuses.</returns>
        Dictionary<SiloAddress, SiloStatus> GetApproximateSiloStatuses(bool onlyActive = false);

        /// <summary>
        /// Gets the name of a silo. 
        /// Silo name is assumed to be static and does not change across restarts of the same silo.
        /// </summary>
        /// <param name="siloAddress">A silo whose name we are interested in.</param>
        /// <param name="siloName">A silo name.</param>
        /// <returns>TTrue if could return the requested name, false otherwise.</returns>
        bool TryGetSiloName(SiloAddress siloAddress, out string siloName);

        /// <summary>
        /// Gets a value indicating whether the current silo is valid for creating new activations on or for directory lookups.
        /// </summary>
        /// <returns>The silo so ask about.</returns>
        bool IsFunctionalDirectory(SiloAddress siloAddress);

        /// <summary>
        /// Gets a value indicating whether the current silo is dead.
        /// </summary>
        /// <returns>The silo so ask about.</returns>
        bool IsDeadSilo(SiloAddress silo);

        /// <summary>
        /// Subscribe to status events about all silos. 
        /// </summary>
        /// <param name="observer">An observer async interface to receive silo status notifications.</param>
        /// <returns>A value indicating whether subscription succeeded or not.</returns>
        bool SubscribeToSiloStatusEvents(ISiloStatusListener observer);

        /// <summary>
        /// UnSubscribe from status events about all silos. 
        /// </summary>
        /// <returns>A value indicating whether subscription succeeded or not.</returns>
        bool UnSubscribeFromSiloStatusEvents(ISiloStatusListener observer);
    }
}
