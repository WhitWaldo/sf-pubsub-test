using System.Collections.Generic;
using Common;
using Microsoft.AspNetCore.Components;

namespace Blazor.Pages
{
    public partial class Home : ComponentBase
    {
        private List<SampleEvent> SampleEvents { get; set; } = new();
        private List<SampleUnorderedEvent> SampleUnorderedEvents { get; set; } = new();

        private void OnUnorderedSampleEventReceived(SampleUnorderedEvent ev)
        {
            SampleUnorderedEvents.Add(ev);
        }

        private void OnSampleEventReceived(SampleEvent ev)
        {
            SampleEvents.Add(ev);
        }
    }
}
