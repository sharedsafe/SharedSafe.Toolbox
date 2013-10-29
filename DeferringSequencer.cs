#if false
using System;
using System.Collections.Generic;

namespace Toolbox
{
	public struct DeferringSequencer
	{
		Queue<Action> _queue;
		public void enqueue(Action action)
		{
			var queue = _queue ?? (_queue = new Queue<Action>());
			queue.Enqueue(action);
			if (queue.Count != 1)
				return;
			do
			{
				try     { queue.Peek()(); }
				finally { queue.Dequeue(); }
			} while (queue.Count != 0);
		}
	};
}

#endif