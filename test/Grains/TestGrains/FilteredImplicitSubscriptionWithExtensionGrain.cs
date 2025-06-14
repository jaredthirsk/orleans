using Microsoft.Extensions.Logging;
using Forkleans.Streams;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [ImplicitStreamSubscription(typeof(RedStreamNamespacePredicate))]
    public class FilteredImplicitSubscriptionWithExtensionGrain : Grain, IFilteredImplicitSubscriptionWithExtensionGrain
    {
        private int counter;
        private readonly ILogger logger;

        public FilteredImplicitSubscriptionWithExtensionGrain(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger($"{nameof(FilteredImplicitSubscriptionWithExtensionGrain)} {IdentityString}");
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("OnActivateAsync");
            var streamProvider = this.GetStreamProvider("SMSProvider");

            var streamIdentity = this.GetImplicitStreamIdentity();
            var stream = streamProvider.GetStream<int>(streamIdentity.Namespace, streamIdentity.Guid);
            await stream.SubscribeAsync(
                (e, t) =>
                {
                    logger.LogInformation("Received a {StreamNamespace} event {Event}", streamIdentity.Namespace, e);
                    ++counter;
                    return Task.CompletedTask;
                });
        }

        public Task<int> GetCounter()
        {
            return Task.FromResult(counter);
        }
    }
}