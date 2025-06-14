using System;
using System.Collections.Generic;
using Forkleans.Runtime;
using Forkleans.Serialization.TypeSystem;

namespace Forkleans.Metadata
{
    /// <summary>
    /// Populates type names on grain properties and grain interface properties.
    /// </summary>
    internal sealed class TypeNameGrainPropertiesProvider : IGrainPropertiesProvider, IGrainInterfacePropertiesProvider
    {
        /// <inheritdoc/>
        public void Populate(Type grainClass, GrainType grainType, Dictionary<string, string> properties)
        {
            properties[WellKnownGrainTypeProperties.TypeName] = grainClass.Name;
            properties[WellKnownGrainTypeProperties.FullTypeName] = grainClass.FullName;
            properties["diag.type"] = RuntimeTypeNameFormatter.Format(grainClass);
            properties["diag.asm"] = CachedTypeResolver.GetName(grainClass.Assembly);
        }

        /// <inheritdoc/>
        public void Populate(Type interfaceType, GrainInterfaceType interfaceId, Dictionary<string, string> properties)
        {
            properties[WellKnownGrainInterfaceProperties.TypeName] = interfaceType.Name;
            properties["diag.type"] = RuntimeTypeNameFormatter.Format(interfaceType);
            properties["diag.asm"] = CachedTypeResolver.GetName(interfaceType.Assembly);
        }
    }
}
