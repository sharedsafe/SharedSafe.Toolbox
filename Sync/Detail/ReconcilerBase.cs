using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	abstract class ReconcilerBase
	{
		public readonly List<IEvent> Events = new List<IEvent>();

		protected void recordSynchronicity(string description, IScope scope, IItem[] items)
		{
			recordEvent(EventType.Synchronicity, description, scope, items);
		}

		protected void recordConflict(string description, IScope scope, IItem[] items)
		{
			recordEvent(EventType.Conflict, description, scope, items);
		}

		protected void recordDeleted(string description, IScope scope, IItem[] items)
		{
			recordEvent(EventType.Deleted, description, scope, items);
		}

		protected void recordInconsistency(string description, IScope scope, IItem[] items)
		{
			recordEvent(EventType.Inconsistency, description, scope, items);
		}


#if false
		protected void recordError(string description, IScope scope, IItem[] items)
		{
			recordEvent(EventType.Error, description, scope, items);
		}
#endif

		#region Event Collection

		void recordEvent(EventType type, string description, IScope scope, IItem[] items)
		{
			var ev = new Event(
				type,
				description,
				makePaths(scope, items),
				items);

			recordEvent(ev);
		}

		void recordEvent(IEvent ev)
		{
			Events.Add(ev);
		}

		#endregion

		#region Helpers

		static IEnumerable<string> makePaths(IScope scope, IItem[] items)
		{
			return from i in items select makePath(scope, i);
		}

		static string makePath(IScope scope, IItem item)
		{
			var itemName = item != null ? item.Name : string.Empty;

			if (scope == null)
				return itemName;

			return scope.makePath() + "/" + itemName;
		}

		#endregion
	}
}
