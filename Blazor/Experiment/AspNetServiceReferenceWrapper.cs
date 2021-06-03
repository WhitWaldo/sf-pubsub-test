using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoCreate.ServiceFabric.PubSub;
using SoCreate.ServiceFabric.PubSub.State;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Blazor.Experiment
{
    public static class SubscriberAspNetExtensions
    {
        public static IServiceCollection AddPubSubSubscriptions(this IServiceCollection services,
            string applicationName, Uri serviceUri, ServicePartitionKind partitionKind)
        {
            var reference = new ServiceReference
            {
                ApplicationName = applicationName,
                ServiceUri = serviceUri,
                PartitionKind = partitionKind,
                ListenerName = "SubscriberStatelessServiceRemotingListener"
            };

            return AddPubSubSubscriptions(services, reference);
        }

        public static IServiceCollection AddPubSubSubscriptions(this IServiceCollection services, ServiceReference serviceReference)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var brokerClient = new BrokerClient();

            var tasks = new List<Task>();
            
            foreach (var subscription in DiscoverHandlers())
            {
                var subscribeAttribute = subscription.Value;
            
                try
                {
                    var subscribeTask = brokerClient.SubscribeAsync(new ServiceReferenceWrapper(serviceReference),
                        subscription.Key, subscribeAttribute.Handler,
                        subscribeAttribute.QueueType == QueueType.Ordered);
                    tasks.Add(subscribeTask);
                    ServiceEventSource.Current.Message($"Registered service: '{serviceReference.ServiceUri}' as subscriber of '{subscription.Key}'");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.Message($"Failed to register service '{serviceReference.ServiceUri}' as a subscriber of '{subscription.Key}'. Error: '{ex.Message}'");
                    throw;
                }
            }
            
            //Actually perform the registrations
            Task.WaitAll(tasks.ToArray());

            //And set up the broker client
            services.AddSingleton<IBrokerClient>(brokerClient);
            //Register the service reference
            services.AddSingleton(serviceReference);

            return services;
        }

        /// <summary>
        /// Finds each of the methods decorated with the <see cref="SubscribeAttribute"/> attribute.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, SubscribeAttribute> DiscoverHandlers()
        {
            var subscribeAttributes = new Dictionary<Type, SubscribeAttribute>();
            var taskType = typeof(Task);
        
            //Get each of the methods in this app domain that are decorated with the Subscribe attribute
        
            var targetTypes = Assembly.GetExecutingAssembly()
                .GetTypes();
        
            foreach (var type in targetTypes)
            {
                if (type == typeof(PubSubStaticHandler))
                {
                    var a = 0;
                }

                var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Instance);

                var methods  = allMethods
                    .Where(a => a.GetCustomAttributes(typeof(SubscribeAttribute), false).Length > 0)
                    .ToList();

                // var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                //     .Where(method => method.GetCustomAttributes(typeof(SubscribeAttribute), false).Length > 0)
                //     .ToList();
        
                if (!methods.Any())
                    continue;
        
                //Iterate through each to identify the attributes and pull their values for the dictionary
                foreach (var method in methods)
                {
                    var subAttribute = method.GetCustomAttributes(typeof(SubscribeAttribute), false)
                        .Cast<SubscribeAttribute>()
                        .SingleOrDefault();
        
                    if (subAttribute == null)
                        continue;
        
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1 || !taskType.IsAssignableFrom(method.ReturnType))
                        continue;
        
                    subAttribute.Handler = m => (Task) method.Invoke(type, new[] {m});
                    subscribeAttributes[parameters[0].ParameterType] = subAttribute;
                }
            }
            
            return subscribeAttributes;
        }
    }
}
