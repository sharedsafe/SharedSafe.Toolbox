using System;
using System.Collections.Generic;
using System.Threading;

namespace Toolbox.Timing
{
	public sealed class AsyncSequencer : IDisposable
	{
		readonly Thread _thread;
		readonly object _syncRoot = new object();
		readonly Queue<Action> _interruptQueue = new Queue<Action>();
		readonly AutoResetEvent _interrupt = new AutoResetEvent(false);

		readonly Sequencer _sequencer = new Sequencer();
		bool _terminate;

		public AsyncSequencer()
		{
			_thread = new Thread(run);
			_thread.Start();
		}

		public void Dispose()
		{
			interrupt(() => _terminate = true);
			_thread.Join();
		}

		public void schedule(long tick, Action action)
		{
			interrupt(() => _sequencer.schedule(tick, action));
		}

		void interrupt(Action action)
		{
			lock (_syncRoot)
			{
				_interruptQueue.Enqueue(action);
				if (_interruptQueue.Count == 1)
					_interrupt.Set();
			}
		}

		void run()
		{
			for(;;)
			{
				var timeToNextEvent = _sequencer.NextEvent;
				if (timeToNextEvent == null)
					_interrupt.WaitOne();
				else
					if (timeToNextEvent.Value != 0)
						_interrupt.WaitOne(TimeSpan.FromTicks(timeToNextEvent.Value));

				// run all interrupts.

				List<Action> allInterrupts;

				lock (_syncRoot)
				{
					allInterrupts = new List<Action>(_interruptQueue);
					_interruptQueue.Clear();
				}

				foreach (var i in allInterrupts)
				{
					try
					{
						i();
						// terminate at the soonest point we detect it!
						if (_terminate)
							return;
					}
					catch (Exception e)
					{
						this.E(e);
					}
				}

				// run sequencer (sequencer has its own error handling).

				_sequencer.run();
			}
		}
	}
}
