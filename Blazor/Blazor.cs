using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using SoCreate.ServiceFabric.PubSub;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Blazor
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class BlazorBase : SubscriberStatelessServiceBase
    {
        private readonly MessageHost _host = new();

        public BlazorBase(StatelessServiceContext serviceContext) : base(serviceContext, null)
        {
            Logger = message => ServiceEventSource.Current.ServiceMessage(serviceContext, message);
        }

        [Subscribe]
        private Task HandleSampleUnorderedEvent(SampleEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(BlazorBase)}");
            _host.AddMessage(ev.Id, ev.Message);
            return Task.CompletedTask;
        }

        [Subscribe(QueueType.Unordered)]
        private Task HandleSampleUnorderedEvent(SampleUnorderedEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(BlazorBase)}");
            _host.AddMessage(ev.Id, ev.Message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                            .UseKestrel()
                            .ConfigureServices(
                                services => services
                                    .AddSingleton<StatelessServiceContext>(serviceContext)
                                    .AddSingleton<MessageHost>(_host)
                                    .AddSingleton<IBrokerClient>(_ => new BrokerClient()))
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseStartup<Startup>()
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseUrls(url)
                            .Build();
                    })),
                new ServiceInstanceListener(context => new FabricTransportServiceRemotingListener(context, this),
                    this.ListenerName)
            };
        }
    }
}
