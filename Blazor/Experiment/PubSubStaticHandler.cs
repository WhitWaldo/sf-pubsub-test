using System;
using System.Threading.Tasks;
using Common;
using SoCreate.ServiceFabric.PubSub.Subscriber;

namespace Blazor.Experiment
{
	public static class PubSubStaticHandler
	{
		public static event Action<SampleEvent> OnReceivedSampleEvent;

		public static event Action<SampleUnorderedEvent> OnReceivedSampleUnorderedEvent;

		[Subscribe]
		private static Task HandleSampleEvent(SampleEvent arg)
		{
			OnReceivedSampleEvent?.Invoke(arg);
			return Task.CompletedTask;
		}

		[Subscribe(QueueType.Unordered)]
		private static Task HandleSampleUnorderedEvent(SampleUnorderedEvent arg)
		{
			OnReceivedSampleUnorderedEvent?.Invoke(arg);
			return Task.CompletedTask;
		}
	}
}
