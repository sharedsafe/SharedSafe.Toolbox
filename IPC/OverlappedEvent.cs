using System;
using System.Threading;

namespace Toolbox.IPC
{
	sealed class OverlappedEvent : IDisposable
	{
		readonly ManualResetEvent _event;
		public readonly Overlapped Overlapped;
		public readonly unsafe NativeOverlapped* Native;
		public readonly IntPtr NativeIntPtr;

		/**
			Create an unsafe overlapped instance using a manual reset event.
		**/

		public static OverlappedEvent use()
		{
			return new OverlappedEvent();
		}

		public void wait()
		{
			if (!wait(null))
				throw this.error("wait Infinite returned false");
		}

		public bool wait(uint? timeout)
		{
			return _event.WaitOne(toSysTimeout(timeout), false);
		}

		// true: overlapped signalled, false: interrupt
		public bool waitInterruptible(WaitHandle interrupt)
		{
			return 0 == WaitHandle.WaitAny(new [] { _event, interrupt });
		}

		public bool wait(uint? timeout, WaitHandle interrupt_)
		{
			if (interrupt_ == null)
				return wait(timeout);

			return 0 == WaitHandle.WaitAny(new[] {_event, interrupt_}, toSysTimeout(timeout));
		}

		unsafe OverlappedEvent()
		{
			_event = new ManualResetEvent(false);
#pragma warning disable 612,618
			// wait handle should be safe here, we Close() the ManualResetEvent on Dispose, 
			// so it can not be GCed as long "Overlapped" is set.
			Overlapped = new Overlapped(0, 0, _event.Handle, null);
#pragma warning restore 612,618
			Native = Overlapped.Pack(null, null);
			NativeIntPtr = new IntPtr(Native);
		}

		#region IDisposable Members

		unsafe public void Dispose()
		{
			// Overlapped.Unpack(Native);
			// just Free it (Unpack is not required)
			Overlapped.Free(Native);
			_event.Close();
		}

		#endregion

		static int toSysTimeout(uint? timeout)
		{
			return timeout == null ? Timeout.Infinite : (int)timeout.Value;
		}
	}
}
