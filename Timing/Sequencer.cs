using System;
using System.Collections.Generic;

namespace Toolbox.Timing
{
	sealed class Sequencer
	{
		readonly LinkedList<Pair<long, Action>> _events = new LinkedList<Pair<long, Action>>();

		public void schedule(long whenInTicks, Action action)
		{
			var current = _events.Last;
			var node = Pair.make(whenInTicks, action);

			while (current != null)
			{
				if (current.Value.First <= whenInTicks)
				{
					_events.AddAfter(current, node);
					return;
				}
				current = current.Previous;
			}

			_events.AddFirst(node);
		}

		// Returns 
		// null if there is no event
		// the ticks until the next event.
		// 0 if run() should be called..

		public long? NextEvent
		{
			get 
			{ 
				var first = _events.First;
				if (first == null)
					return null;

				var now = Ticks.Current;
				var ev = first.Value.First;
				if (ev <= now)
					return 0;
				return ev - now;
			}
		}

		public void run()
		{
			while (shallRunFirst())
			{
				var first = _events.First;
				_events.Remove(first);
				var action = first.Value.Second;
				try
				{
					action();
				}
				catch (Exception e)
				{
					this.E(e);
				}
			}
		}

		bool shallRunFirst()
		{
			var next = NextEvent;
			return next!= null && next.Value == 0;
		}
	}
}
