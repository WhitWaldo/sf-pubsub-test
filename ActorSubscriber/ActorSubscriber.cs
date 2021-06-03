using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using SoCreate.ServiceFabric.PubSub;
using SoCreate.ServiceFabric.PubSub.State;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace ActorSubscriber
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class ActorSubscriber : Actor, ISubscriberActor
    {
        private readonly IBrokerClient _brokerClient;

        /// <summary>
        /// Initializes a new instance of ActorSubscriber
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ActorSubscriber(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
            _brokerClient = new BrokerClient(null);
        }
        
        public Task ReceiveMessageAsync(MessageWrapper message)
        {
            return _brokerClient.ProcessMessageAsync(message);
        }

        public Task Subscribe()
        {
            return Task.WhenAll(
                _brokerClient.SubscribeAsync<SampleEvent>(this, HandleMessageSampleEvent, true),
                _brokerClient.SubscribeAsync<SampleUnorderedEvent>(this, HandleMessageSampleUnorderedEvent, false));
        }

        private Task HandleMessageSampleEvent(SampleEvent ev)
        {
            ActorEventSource.Current.ActorMessage(this, $"Processing {ev.GetType()}: {ev.Message} on {nameof(ActorSubscriber)}");
            return Task.CompletedTask;
        }

        private Task HandleMessageSampleUnorderedEvent(SampleUnorderedEvent ev)
        {
            ActorEventSource.Current.ActorMessage(this, $"Processing {ev.GetType()}: {ev.Message} on {nameof(ActorSubscriber)}");
            return Task.CompletedTask;
        }
    }
}
