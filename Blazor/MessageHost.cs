using System;
using System.Collections.Generic;

namespace Blazor
{
	public class MessageHost
	{
		public Dictionary<Guid, string> Messages { get; set; } = new();
		
		public event EventHandler MessagesUpdated;

		public void AddMessage(Guid id, string message)
		{
			//For reasons yet unknown, this _sometimes_ yields duplicate values in the Blazor service that aren't seen in the Stateless1 service
			if (!Messages.ContainsKey(id))	
				Messages.Add(id, message);
				
			MessagesUpdated?.Invoke(null, new EventArgs());
		}
	}
}
