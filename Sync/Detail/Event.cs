using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	sealed class Event : IEvent
	{
		public Event(EventType type, string description, IEnumerable<string> paths, IEnumerable<IItem> items)
		{
			Severity = SeverityTable[(int)type];
			Time = DateTime.UtcNow;
			Type = type;
			Description = description;
			Paths = paths.ToArray();
			Items = items.ToArray();
		}

		#region IEvent Members

		public EventSeverity Severity { get; private set; }
		public DateTime Time { get; private set; }
		public EventType Type { get; private set; }
		public string Description { get; private set; }
		public IEnumerable<string> Paths { get; private set; }
		public IEnumerable<IItem> Items  { get; private set; }

		public Exception Exception { get; set; }

		#endregion


		static readonly EventSeverity[] SeverityTable = new EventSeverity[] 
		{ 
			EventSeverity.Error, 
			EventSeverity.Warning, 
			EventSeverity.Warning, 
			EventSeverity.Info, 
			EventSeverity.Info
		};
	}
}
