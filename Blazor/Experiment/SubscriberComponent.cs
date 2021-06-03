using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SoCreate.ServiceFabric.PubSub;
using SoCreate.ServiceFabric.PubSub.State;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Blazor.Experiment
{
    public abstract class SubscriberComponent : ComponentBase, IDisposable, ISubscriberService
    {
        //Register when the component is created
        [Inject]
        protected IBrokerClient BrokerClient { get; set; }

        [Inject]
        protected ServiceReference ServiceReference { get; set; }
        
        protected override async Task OnInitializedAsync()
        {
            //Identify each of the subscribe methods on this class
            foreach (var sub in DiscoverHandlers())
            {
                var attr = sub.Value;

                try
                {
                    await BrokerClient.SubscribeAsync(new ServiceReferenceWrapper(ServiceReference), sub.Key,
                        attr.Handler, attr.QueueType == QueueType.Ordered);
                    ServiceEventSource.Current.Message(
                        $"Registered service: '{ServiceReference.ServiceUri}' as subscriber of '{sub.Key}'");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.Message($"Failed to register service '{ServiceReference.ServiceUri}' as subscriber of '{sub.Key}' with error '{ex.Message}'");
                    throw;
                }
            }

            await base.OnInitializedAsync();
        }

        private Dictionary<Type, SubscribeAttribute> DiscoverHandlers()
        {
            var subscribeAttributes = new Dictionary<Type, SubscribeAttribute>();
            var taskType = typeof(Task);

            var methods = GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var subscribeAttribute = method.GetCustomAttributes(typeof(SubscribeAttribute), false)
                    .Cast<SubscribeAttribute>()
                    .SingleOrDefault();

                if (subscribeAttribute == null)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1 || !taskType.IsAssignableFrom(method.ReturnType))
                    continue;

                var constructor = this.GetType().GetConstructor(Type.EmptyTypes);
                subscribeAttribute.Handler = m => (Task)method.Invoke(constructor, new[] {m});
                
                //subscribeAttribute.Handler = m => (Task) method.Invoke(this, new[] {m});
                subscribeAttributes[parameters[0].ParameterType] = subscribeAttribute;
            }

            return subscribeAttributes;
        }

        
        public void Dispose()
        {
            var tasks = new List<Task>();
            foreach (var sub in DiscoverHandlers())
            {
                try
                {
                    var task = BrokerClient.UnsubscribeAsync(new ServiceReferenceWrapper(ServiceReference), sub.Key);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        public virtual Task ReceiveMessageAsync(MessageWrapper message)
        {
            return BrokerClient.ProcessMessageAsync(message);
        }
    }
}
