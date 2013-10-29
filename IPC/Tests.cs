#if DEBUG

using System;
using System.Threading;
using NUnit.Framework;


namespace Toolbox.IPC
{
	[TestFixture]
	public class Tests
	{
		/**
			A disconnection of a pipe while in a Receive call is no exception and so should be indicated
			as a return code (zero bytes read).

			We changed this semantic from the original Mono Pipe implementation to simply error logging and
			handling.
		**/

		[Test]
		public void testDisconnectionRead0()
		{
			var listener = new NamedPipeListener("TestPipe");
			var sig = new AutoResetEvent(false);

			ThreadPool.QueueUserWorkItem(state =>
				{
					var connectedSocket = listener.Accept();
					var buf = new byte[100];

					var r = connectedSocket.Receive(buf, 0, 100);

					if (r != 0)
						throw new Exception("Expecing a zero return in Receive when the other side closes the connection.");

					// this sig should be triggered.
					sig.Set();
				});

			// wait until the pipe is up.
			Thread.Sleep(250);

			var socket = NamedPipeClient.tryConnect("TestPipe");
			socket.Close();

			sig.WaitOne();
		}

		[Test]
		public void testDisconnectionReadObject()
		{
			var listener = new ObjectServer("TestPipe");
			var sig = new AutoResetEvent(false);

			ThreadPool.QueueUserWorkItem(state =>
			{
				var connectedSocket = listener.tryAccept();
				var buf = new byte[100];

				var r = connectedSocket.tryReceive<Tests>((uint?)null);
				if (r != null)
					throw new Exception("Received unexpected object");

				// this sig should be triggered.
				sig.Set();
			});

			// wait until the pipe is up.
			Thread.Sleep(250);

			var socket = ObjectEndPoint.tryConnect("TestPipe");
			socket.Dispose();
			
			sig.WaitOne();
		}
	}
}

#endif