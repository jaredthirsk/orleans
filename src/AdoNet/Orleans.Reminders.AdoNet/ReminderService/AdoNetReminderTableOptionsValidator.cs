using Microsoft.Extensions.Options;
using Forkleans.Runtime;
using Forkleans.Runtime.ReminderService;

namespace Forkleans.Configuration
{
    /// <summary>
    /// Validates <see cref="AdoNetReminderTableOptions"/> configuration.
    /// </summary>
    public class AdoNetReminderTableOptionsValidator : IConfigurationValidator
    {
        private readonly AdoNetReminderTableOptions options;
        
        public AdoNetReminderTableOptionsValidator(IOptions<AdoNetReminderTableOptions> options)
        {
            this.options = options.Value;
        }

        /// <inheritdoc />
        public void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(this.options.Invariant))
            {
                throw new ForkleansConfigurationException($"Invalid {nameof(AdoNetReminderTableOptions)} values for {nameof(AdoNetReminderTable)}. {nameof(options.Invariant)} is required.");
            }

            if (string.IsNullOrWhiteSpace(this.options.ConnectionString))
            {
                throw new ForkleansConfigurationException($"Invalid {nameof(AdoNetReminderTableOptions)} values for {nameof(AdoNetReminderTable)}. {nameof(options.ConnectionString)} is required.");
            }
        }
    }
}