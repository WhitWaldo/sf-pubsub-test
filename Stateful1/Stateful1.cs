using System.Fabric;
using System.Threading.Tasks;
using Common;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Stateful1
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateful1 : SubscriberStatefulServiceBase
    {
        public Stateful1(StatefulServiceContext context)
            : base(context, null)
        {
            Logger = message => ServiceEventSource.Current.ServiceMessage(context, message);
        }

        [Subscribe]
        private Task HandleSampleEvent(SampleEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(Stateful1)}");
            return Task.CompletedTask;
        }

        [Subscribe(QueueType.Unordered)]
        private Task HandleSampleUnorderedEvent(SampleUnorderedEvent ev)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing {ev.GetType()}: {ev.Message} on {nameof(Stateful1)}");
            return Task.CompletedTask;
        }
    }
}
