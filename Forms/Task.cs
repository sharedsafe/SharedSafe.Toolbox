using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Toolbox.Forms
{
	public interface ITask : IDisposable
	{
		void interrupt();
	}

	public interface ITask<ResultT> : ITask
	{
		bool IsCompleted { get; }
		bool tryGetResult(out ResultT res);
	}

	public interface ITask<EventT, ResultT> : ITask<ResultT>
	{
		bool tryGetEvent(out EventT ev);
		Two<bool> tryGetProgress(out EventT ev, out ResultT res);
	}

	public interface ITaskServer
	{
		WaitHandle Interrupt { get; }
	}

	public interface ITaskServer<EventT> : ITaskServer
	{
		void notify(EventT ev);
	}

	public static class Task
	{
		struct NullT { };

		public static ITask<ResultT> run<ResultT>(Func<ITaskServer, ResultT> f)
		{
			var tsk = new Task<NullT, ResultT>(s => f(s));
			tsk.run();
			return tsk;
		}

		public static ITask<EventT, ResultT> run<EventT, ResultT>(Func<ITaskServer<EventT>, ResultT> f)
		{
			var tsk = new Task<EventT, ResultT>(f);
			tsk.run();
			return tsk;
		}
	}
	
	/**
		Asynchronous task implementation.
	**/

	sealed class Task<EventT, ResultT> : ITaskServer<EventT>, ITask<EventT, ResultT>, IDisposable
	{
		readonly Func<ITaskServer<EventT>, ResultT> _f;
		readonly Thread _thread;
		readonly ManualResetEvent _interrupt = new ManualResetEvent(false);

		readonly object _sync = new object();
			bool _running;	
			bool _resultValid;
			ResultT _result;
			Queue<EventT> _events;
			Exception _error_;


		#region IDisposable Members

		public Task(Func<ITaskServer<EventT>, ResultT> func)
		{
			_f = func;

			_thread = new Thread(thread);
		}

		public void Dispose()
		{
			interrupt();
			_thread.Join();
		}

		public void run()
		{
			lock (_sync)
			{
				Debug.Assert(!_running, "Task was already started, run called twice.");
				_running = true;
			}
			_thread.Start();
		}

		#region Client Interface

		public void interrupt()
		{
			_interrupt.Set();
		}

		public bool tryGetEvent(out EventT ev)
		{
			lock (_sync)
			{
				invariantInSync();
				
				if (_events != null && _events.Count != 0)
				{
					ev = _events.Dequeue();
					return true;
				}
			}

			ev = default(EventT);
			return false;
		}

		public bool IsCompleted
		{
			get 
			{
				lock (_sync)
				{
					return _resultValid || _error_ != null;
				}
			}
		}

		public bool tryGetResult(out ResultT res)
		{
			lock (_sync)
			{
				invariantInSync();

				if (!_resultValid)
				{
					res = default(ResultT);
					return false;
				}
				res = _result;
				return true;
			}
		}

		public Two<bool> tryGetProgress(out EventT ev, out ResultT res)
		{
			var b1 = tryGetEvent(out ev);
			var b2 = tryGetResult(out res);
			return Two.make(b1, b2);
		}

		#endregion

		void invariantInSync()
		{
			if (_interrupt.WaitOne(0))
				throw new InternalError("Thread aborting, no more events or result.");

			if (_error_ != null)
				throw _error_;
		}

		void thread()
		{
			try
			{
				var res = _f(this);

				lock (_sync)
				{
					if (_interrupt.WaitOne(0))
						return;

					_result = res;
					_resultValid = true;
				}
			}
			catch (Exception e)
			{
				lock (_sync)
				{
					_error_ = e;
				}
			}
		}

		#region Server Interface

		public void notify(EventT ev)
		{
			lock (_sync)
			{
				if (_events == null)
					_events = new Queue<EventT>();

				_events.Enqueue(ev);
			}
		}

		public WaitHandle Interrupt
		{
			get 
			{
				return _interrupt;
			}
		}

		#endregion

		#endregion

	}
}
