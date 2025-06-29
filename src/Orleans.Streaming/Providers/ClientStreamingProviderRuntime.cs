using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Forkleans.Runtime;
using Forkleans.Streams;
using Microsoft.Extensions.Logging;

namespace Forkleans.Providers
{
    internal class ClientStreamingProviderRuntime : IStreamProviderRuntime, ILifecycleParticipant<IClusterClientLifecycle>
    {
        private readonly IStreamPubSub grainBasedPubSub;
        private readonly IStreamPubSub implicitPubSub;
        private readonly IStreamPubSub combinedGrainBasedAndImplicitPubSub;
        private StreamDirectory streamDirectory;
        private readonly IInternalGrainFactory grainFactory;
        private readonly ImplicitStreamSubscriberTable implicitSubscriberTable;
        private readonly ClientGrainContext clientContext;
        private readonly IRuntimeClient runtimeClient;

        public ClientStreamingProviderRuntime(
            IInternalGrainFactory grainFactory,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ImplicitStreamSubscriberTable implicitSubscriberTable,
            ClientGrainContext clientContext)
        {
            this.grainFactory = grainFactory;
            this.ServiceProvider = serviceProvider;
            this.implicitSubscriberTable = implicitSubscriberTable;
            this.clientContext = clientContext;
            this.runtimeClient = serviceProvider.GetService<IRuntimeClient>();
            grainBasedPubSub = new GrainBasedPubSubRuntime(GrainFactory);
            var tmp = new ImplicitStreamPubSub(this.grainFactory, this.implicitSubscriberTable);
            implicitPubSub = tmp;
            combinedGrainBasedAndImplicitPubSub = new StreamPubSubImpl(grainBasedPubSub, tmp);
            streamDirectory = new StreamDirectory();
        }

        public IGrainFactory GrainFactory => this.grainFactory;

        public IServiceProvider ServiceProvider { get; }

        public StreamDirectory GetStreamDirectory()
        {
            return streamDirectory;
        }

        public async Task Reset(bool cleanup = true)
        {
            if (streamDirectory != null)
            {
                var tmp = streamDirectory;
                streamDirectory = null; // null streamDirectory now, just to make sure we call cleanup only once, in all cases.
                if (cleanup)
                {
                    await tmp.Cleanup(true, true);
                }
            }
        }

        public string ExecutingEntityIdentity()
        {
            return this.runtimeClient.CurrentActivationIdentity;
        }

        public (TExtension, TExtensionInterface) BindExtension<TExtension, TExtensionInterface>(Func<TExtension> newExtensionFunc)
            where TExtension : class, TExtensionInterface
            where TExtensionInterface : class, IGrainExtension
        {
            return this.clientContext.GetOrSetExtension<TExtension, TExtensionInterface>(newExtensionFunc);
        }

        public IStreamPubSub PubSub(StreamPubSubType pubSubType)
        {
            switch (pubSubType)
            {
                case StreamPubSubType.ExplicitGrainBasedAndImplicit:
                    return combinedGrainBasedAndImplicitPubSub;
                case StreamPubSubType.ExplicitGrainBasedOnly:
                    return grainBasedPubSub;
                case StreamPubSubType.ImplicitOnly:
                    return implicitPubSub;
                default:
                    return null;
            }
        }

        public void Participate(IClusterClientLifecycle lifecycle)
        {
            lifecycle.Subscribe<ClientStreamingProviderRuntime>(ServiceLifecycleStage.RuntimeInitialize,
                ct => Task.CompletedTask,
                async ct => await this.Reset(!ct.IsCancellationRequested));
        }
    }
}
