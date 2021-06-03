using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SoCreate.ServiceFabric.PubSub;

namespace Blazor
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class BlazorBase : StatelessService
    {
        private string _listenerName { get; set; } = "SubscriberStatelessServiceRemotingListener";

        public BlazorBase(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }

        // private readonly IBrokerClient _brokerClient;
        //
        // private Action<string> Logger { get; set; }
        //
        // public BlazorBase(StatelessServiceContext context)
        //     : base(context)
        // {
        //     _brokerClient = new BrokerClient();
        //
        //     //Set the logger
        //     Logger = msg => ServiceEventSource.Current.ServiceMessage(context, msg);
        // }
        //
        // protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        // {
        //     for (var attempt = 0;; attempt++)
        //     {
        //         try
        //         {
        //             await Subscribe();
        //             break;
        //         }
        //         catch (BrokerNotFoundException)
        //         {
        //             if (attempt > 10)
        //                 throw;
        //
        //             await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        //         }
        //     }
        // }
        //
        // private async Task Subscribe()
        // {
        //
        // }
        
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
                                    .AddSingleton<IBrokerClient>(_ => new BrokerClient()))
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseStartup<Startup>()
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseUrls(url)
                            .Build();
                    }))
            };
        }
    }
}
