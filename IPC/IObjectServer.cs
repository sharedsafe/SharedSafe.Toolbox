using System;
using System.Diagnostics;
using System.Threading;

namespace Toolbox.IPC
{
	public interface IObjectServer : IDisposable
	{
		/// Returns the IObjectEndPoint or null if the server was aborted.
		IObjectEndPoint tryAccept(uint? timeout, WaitHandle interrupt_);
	}

	public static class ObjectServerExtensions
	{
		public static IObjectEndPoint tryAccept(this IObjectServer server)
		{
			return server.tryAccept(null, null);
		}

		public static IObjectEndPoint tryAccept(this IObjectServer server, uint? timeout)
		{
			return server.tryAccept(timeout, null);
		}

		public static IObjectEndPoint tryAccept(this IObjectServer server, WaitHandle interrupt)
		{
			Debug.Assert(interrupt != null);
			return server.tryAccept(null, interrupt);
		}
	}
}
