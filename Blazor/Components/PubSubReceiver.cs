using System;
using Blazor.Experiment;
using Common;
using Microsoft.AspNetCore.Components;

namespace Blazor.Components
{
    public class PubSubReceiver : SubscriberComponent, IDisposable
    {
        [Parameter]
        public EventCallback<SampleEvent> OnSampleEventReceived { get; set; } = new();

        [Parameter]
        public EventCallback<SampleUnorderedEvent> OnSampleUnorderedEventReceived { get; set; } = new();

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            PubSubStaticHandler.OnReceivedSampleEvent += HandleSampleEvent;
            PubSubStaticHandler.OnReceivedSampleUnorderedEvent += HandleSampleUnorderedEvent;
        }
        

        private void HandleSampleEvent(SampleEvent arg)
        {
            OnSampleEventReceived.InvokeAsync(arg);
        }
        
        private void HandleSampleUnorderedEvent(SampleUnorderedEvent arg)
        {
            OnSampleUnorderedEventReceived.InvokeAsync(arg);
        }

        public void Dispose()
        {
            PubSubStaticHandler.OnReceivedSampleEvent -= HandleSampleEvent;
            PubSubStaticHandler.OnReceivedSampleUnorderedEvent -= HandleSampleUnorderedEvent;
        }
    }
}
