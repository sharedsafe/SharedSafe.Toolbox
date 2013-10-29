using System;
using System.Collections.Generic;

namespace Toolbox.Sync
{
	public enum EventSeverity
	{
		Error,
		Warning,
		Info
	};

	/// WARNING: must update SeverityTable when this Table is extended.
	/// todo: Need Enum Map support in Toolbox

	public enum EventType
	{
		/// Error: Error in file relocation.
		RelocationError,

		/// Warning: Conflict
		Conflict,

		/// Warning: Inconcistency, some other errors caused the file system to be inconsistent
		Inconsistency,

		/// Info: In the regular synchronization process, some content has been deleted
		Deleted,

		/// Info: Changes are detected which are in both replicas.
		Synchronicity
	}

	public interface IEvent
	{
		/// How important is this event.
		EventSeverity Severity { get; }

		/// (Utc) Time of event.
		DateTime Time { get; }

		string Description { get; }

		/// Description of event
		EventType Type { get; }

		/// Paths involved.
		IEnumerable<string> Paths { get; }

		/// Items involved.
		IEnumerable<IItem> Items { get; }

		/// Associated system error (if any, may be null)
		Exception Exception { get; }
	}
}
