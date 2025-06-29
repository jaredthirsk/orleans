using Forkleans.Runtime;
using System.Collections.Generic;


namespace Forkleans.Storage
{
    /// <summary>
    /// A picker to choose from provided hash functions. Provides agility to update or change hashing functionality for both built-in and custom operations.
    /// </summary>
    /// <remarks>The returned hash needs to be thread safe or a unique instance.</remarks>
    public interface IStorageHasherPicker
    {
        /// <summary>
        /// The hash functions saved to this picker.
        /// </summary>
        ICollection<IHasher> HashProviders { get; }

        /// <summary>Picks a hasher using the given parameters.</summary>
        /// <param name="serviceId">The ID of the current service.</param>
        /// <param name="storageProviderInstanceName">The requesting storage provider.</param>
        /// <param name="grainType">The type of grain.</param>
        /// <param name="grainId">The grain ID.</param>
        /// <param name="grainState">The grain state.</param>
        /// <param name="tag">An optional tag parameter that might be used by the storage parameter for "out-of-band" contracts.</param>
        /// <returns>A serializer or <em>null</em> if not match was found.</returns>
        IHasher PickHasher<T>(string serviceId, string storageProviderInstanceName, string grainType, GrainId grainId, IGrainState<T> grainState, string tag = null);
    }
}
