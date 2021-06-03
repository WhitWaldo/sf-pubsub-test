using System.Fabric;
using System.Threading.Tasks;
using Common;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Stateless1
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless1 : SubscriberStatelessServiceBase
    {
        public Stateless1(StatelessServiceContext context) : base(context, null)
        {
            Logger = message => ServiceEventSource.Current.ServiceMessage(context, message);
        }

        [Subscribe]
        private Task HandleSampleEvent(SampleEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(Stateless1)}");
            return Task.CompletedTask;
        }

        [Subscribe(QueueType.Unordered)]
        private Task HandleSampleUnorderedEvent(SampleUnorderedEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(Stateless1)}");
            return Task.CompletedTask;
        }
    }
}
