using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	/**
		A Windows.Forms dispatcher to run Actions in an Application's message queue.

		User Dispatcher.Current to retrieve the dispatcher for the current thread, and
		Dispatcher.dispatch() to dispatch any arbitrary action to that thread.

		@note
			There is not consideration for Exceptions right now.
	**/

	public sealed class Dispatcher : IMessageFilter
	{
		#region Public Methods

		/// Resolve the dispatcher attached to the current thread.

		public static Dispatcher Current
		{
			get { return resolve(); }
		}

		public static void run()
		{
			Application.Run();
		}

		const uint WM_QUIT = 0x0012;

		public static void postQuit(uint nativeThreadId)
		{
			PostThreadMessage(nativeThreadId, WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
		}

		static Dispatcher resolve()
		{
			if (ThreadDispatcher != null)
				return ThreadDispatcher;

			// todo: initialized dispatcher lazy when an asynchronous message is sent!!!!
			// caller thread must have a message loop!
			// install message filter on this thread
#pragma warning disable 618,612
			var currentNativeThreadId = (uint)AppDomain.GetCurrentThreadId();
#pragma warning restore 618,612
			ThreadDispatcher = new Dispatcher(currentNativeThreadId);
			Application.AddMessageFilter(ThreadDispatcher);

			// note: for some reasone, there is a bug, we need help from the
			// idle handler to schedule all our actions, sometimes thread messages are simply going lost when
			// other messages are sent.

			ApplicationExtensions.onIdle(ThreadDispatcher.runToCompletion);
			
			return ThreadDispatcher;
		}
		
		public void dispatch(Action action)
		{
			lock (_lock)
			{
				_actions.Enqueue(action);
				if (_actions.Count == 1)
					PostThreadMessage(_nativeThreadId, _winMsg, UIntPtr.Zero, IntPtr.Zero);
			}
		}

		#endregion

		[ThreadStatic]
		static Dispatcher ThreadDispatcher;

		readonly Queue<Action> _actions = new Queue<Action>();
		readonly object _lock = new object();
		readonly uint _nativeThreadId;

		Dispatcher(uint nativeThreadId)
		{
			_nativeThreadId = nativeThreadId;
		}

		#region IMessageFilter Members

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			if (m.Msg != _winMsg)
				return false;

			runToCompletion();

			return true; // don't dispatch me
		}

		public void runToCompletion()
		{
			// note: can't lock the queue while we are running the actions, we may end up with 
			// deadlocking someone that enqueues.

			Action next_;

			while (null != (next_ = tryDequeueNextAction()))
				next_();
		}

		Action tryDequeueNextAction()
		{
			lock (_lock)
			{
				if (_actions.Count == 0)
					return null;
				return _actions.Dequeue();
			}
		}

		#endregion

		#region Static Initialization

		static readonly uint _winMsg;

		static Dispatcher()
		{
			_winMsg = RegisterWindowMessage(typeof(Dispatcher).FullName);
		}

		#endregion

		#region Imports

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint RegisterWindowMessage(string lpString);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool PostThreadMessage(uint threadId, uint msg, UIntPtr wParam, IntPtr lParam);

		#endregion
	}
}
