using System;
using System.Threading;
using Toolbox.Forms;

namespace Toolbox
{
	/**
		Simple, asynchronous thread / communication library.

		Uses the Context<> and Dispatcher components.
	**/

	public static class Async
	{
		/**
			Runs an asynchronous task with cancellation support but no notifications.
		**/

		public static IAsync run(Action action)
		{
			return new AsyncImpl(action, null);
		}

		/**
			Runs an asynchronous task and dispatches its notification to the action that is provided in the
			thread context of the caller! (This is done using the a forms dispatcher, and so requires a windows
			forms application right now).
		**/

		public static IAsync runAndDispatch<NotificationT>(Action action, Action<NotificationT> notification)
			where NotificationT : class
		{
			var dispatcher = Dispatcher.Current;

			return new AsyncImpl(action,
				ev => dispatcher.dispatch(
					() =>
						{
							var not = ev as NotificationT;
							if (not != null)
								notification(not);
						}));
		}

		#region Asynchronous Task Operations

		/// Tests if the task should cancel.

		public static bool Cancelled
		{
			get 
			{
				return Context<AsyncImpl>.Current.Cancelling;
			}
		}

		/// Sends out a notification.

		public static void notify(object ev)
		{
			Context<AsyncImpl>.Current.notify(ev);
		}

		#endregion
	}

	sealed class AsyncImpl : IAsync
	{
		readonly Thread _thread;
		readonly Action _action;
		readonly Action<object> _eventNotificationTarget;
		Exception _exception;
		readonly object _lock = new object();
			bool _cancelling;

		#region Startup / Shutdown

		public AsyncImpl(Action action, Action<object> eventNotificationTarget)
		{
			_action = action;
			_eventNotificationTarget = eventNotificationTarget;

			_thread = new Thread(run);
			_thread.Start();
		}

		public void Dispose()
		{
			_thread.Join();

			// no exception is thrown to the caller if the caller cancelled!

			if (!_cancelling && _exception != null)
				throw _exception;
		}

		#endregion

		#region Cancelling

		public bool Cancelling 
		{
			get
			{
				lock (_lock)
				{
					return _cancelling;
				}
			}
		}

		public void cancel()
		{
			lock (_lock)
			{
				if (_cancelling)
					this.W("cancel() called twice");
				_cancelling = true;
			}
		}

		#endregion

		void run()
		{
			try
			{
				using (Context.push(this))
					_action();
			}
			catch (Exception e)
			{
				_exception = e;
			}
		}

		#region Notifications

		public void notify(object ev)
		{
			// if a event notification target is set, we directly deliver the event!

			if (_eventNotificationTarget == null)
				return;

			_eventNotificationTarget(ev);
		}

		#endregion
	}

	public interface IAsync : IDisposable
	{
		/// Cancels the asynchronous thread and returns when the thread has been canceled.
		void cancel();

		bool Cancelling { get; }
	}
}
