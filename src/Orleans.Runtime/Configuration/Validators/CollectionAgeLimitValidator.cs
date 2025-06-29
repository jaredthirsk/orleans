using System;
using Microsoft.Extensions.Options;
using Forkleans.Runtime;

namespace Forkleans.Configuration
{
    internal class GrainCollectionOptionsValidator : IConfigurationValidator
    {
        private readonly IOptions<GrainCollectionOptions> options;

        public GrainCollectionOptionsValidator(IOptions<GrainCollectionOptions> options)
        {
            this.options = options;
        }

        public void ValidateConfiguration()
        {
            if (this.options.Value.CollectionQuantum <= TimeSpan.Zero)
            {
                throw new ForkleansConfigurationException(
                    $"{nameof(GrainCollectionOptions.CollectionQuantum)} is set to {options.Value.CollectionQuantum}. " +
                    $"{nameof(GrainCollectionOptions.CollectionQuantum)} must be greater than 0");
            }

            if (this.options.Value.CollectionAge <= this.options.Value.CollectionQuantum)
            {
                throw new ForkleansConfigurationException(
                    $"{nameof(GrainCollectionOptions.CollectionAge)} is set to {options.Value.CollectionAge}. " +
                    $"{nameof(GrainCollectionOptions.CollectionAge)} must be greater than {nameof(GrainCollectionOptions.CollectionQuantum)}, " +
                    $"which is set to {this.options.Value.CollectionQuantum}");
            }
            foreach(var classSpecificCollectionAge in this.options.Value.ClassSpecificCollectionAge)
            {
                if (classSpecificCollectionAge.Value <= this.options.Value.CollectionQuantum)
                {
                    throw new ForkleansConfigurationException(
                        $"{classSpecificCollectionAge.Key} CollectionAgeLimit is set to {classSpecificCollectionAge.Value}. " +
                        $"CollectionAgeLimit must be greater than {nameof(GrainCollectionOptions.CollectionQuantum)}, " +
                        $"which is set to {this.options.Value.CollectionQuantum}");
                }
            }
        }
    }
}
