using System;
using System.Collections.Generic;
using Forkleans.Metadata;
using Forkleans.Runtime;
using static Forkleans.Placement.ImmovableAttribute;

namespace Forkleans.Placement
{
    /// <summary>
    /// Base for all placement policy marker attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class PlacementAttribute : Attribute, IGrainPropertiesProviderAttribute
    {
        public PlacementStrategy PlacementStrategy { get; private set; }

        protected PlacementAttribute(PlacementStrategy placement)
        {
            if (placement == null) throw new ArgumentNullException(nameof(placement));

            this.PlacementStrategy = placement;
        }

        /// <inheritdoc />
        public virtual void Populate(IServiceProvider services, Type grainClass, GrainType grainType, Dictionary<string, string> properties)
        {
            this.PlacementStrategy?.PopulateGrainProperties(services, grainClass, grainType, properties);
        }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="RandomPlacement"/> policy.
    /// </summary>
    /// <remarks>
    /// This is the default placement policy, so this attribute does not need to be used for normal grains.
    /// </remarks>
    /// <inheritdoc cref="RandomPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RandomPlacementAttribute : PlacementAttribute
    {
        public RandomPlacementAttribute() :
            base(RandomPlacement.Singleton)
        {
        }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="HashBasedPlacement"/> policy.
    /// </summary>
    /// <inheritdoc cref="HashBasedPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HashBasedPlacementAttribute : PlacementAttribute
    {
        public HashBasedPlacementAttribute() :
            base(HashBasedPlacement.Singleton)
        { }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="PreferLocalPlacement"/> policy.
    /// </summary>
    /// <inheritdoc cref="PreferLocalPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PreferLocalPlacementAttribute : PlacementAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferLocalPlacementAttribute"/> class.
        /// </summary>
        public PreferLocalPlacementAttribute()
            : base(PreferLocalPlacement.Singleton)
        {
        }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="ActivationCountBasedPlacement"/> policy, which attempts to balance
    /// grain placement across servers based upon the relative number of recently active grains on each one.
    /// </summary>
    /// <inheritdoc cref="ActivationCountBasedPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ActivationCountBasedPlacementAttribute : PlacementAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationCountBasedPlacementAttribute"/> class.
        /// </summary>
        public ActivationCountBasedPlacementAttribute()
            : base(ActivationCountBasedPlacement.Singleton)
        {
        }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="SiloRoleBasedPlacement"/> policy.
    /// </summary>
    /// <inheritdoc cref="SiloRoleBasedPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SiloRoleBasedPlacementAttribute : PlacementAttribute
    {
        public SiloRoleBasedPlacementAttribute() :
            base(SiloRoleBasedPlacement.Singleton)
        { }
    }

    /// <summary>
    /// Marks a grain class as using the <see cref="ResourceOptimizedPlacement"/> policy.
    /// </summary>
    /// <inheritdoc cref="ResourceOptimizedPlacement"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ResourceOptimizedPlacementAttribute : PlacementAttribute
    {
        public ResourceOptimizedPlacementAttribute() :
            base(ResourceOptimizedPlacement.Singleton)
        { }
    }

    /// <summary>
    /// Ensures that activations of this grain type will not be migrated automatically.
    /// </summary>
    /// <remarks>Activations can still be migrated by user initiated code.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ImmovableAttribute(ImmovableKind kind = ImmovableKind.Any)
        : Attribute, IGrainPropertiesProviderAttribute
    {
        /// <summary>
        /// The kind of immovability.
        /// </summary>
        public ImmovableKind Kind { get; } = kind;

        /// <inheritdoc/>
        public void Populate(IServiceProvider services, Type grainClass, GrainType grainType, Dictionary<string, string> properties)
            => properties[WellKnownGrainTypeProperties.Immovable] = ((byte)Kind).ToString();
    }

    /// <summary>
    /// Emphasizes that immovability is restricted to certain components.
    /// </summary>
    [Flags]
    public enum ImmovableKind : byte
    {
        /// <summary>
        /// Activations of this grain type will not be migrated by the repartitioner.
        /// </summary>
        Repartitioner = 1,
        /// <summary>
        /// Activations of this grain type will not be migrated by the rebalancer.
        /// </summary>
        Rebalancer = 2,
        /// <summary>
        /// Activations of this grain type will not be migrated by anything.
        /// </summary>
        Any = Repartitioner | Rebalancer
    }
}
