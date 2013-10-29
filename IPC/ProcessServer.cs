/**
	Create a new process IPC server.

	Note: if a process IPC server with the same name is already instantiated, an exception is thrown.
	The protection is established by creating a named mutex.
**/

using System.Threading;

namespace Toolbox.IPC
{
	sealed class ProcessServer : IObjectServer
	{
		readonly string _endPointName;
		readonly Mutex _mutex;
		readonly IObjectServer _objectServer;

		public static ProcessServer tryCreate(string processPath)
		{
			var endPointName = ProcessHelper.replaceInvalidFileNameCharacters(processPath);
			bool createdNew;
			var mutex = new Mutex(false, "Toolbox.IPC.ProcessServer.Mutex." + endPointName, out createdNew);
			return !createdNew ? null : new ProcessServer(endPointName, mutex);
		}

		ProcessServer(string endPointName, Mutex mutex)
		{
			_endPointName = endPointName;
			_mutex = mutex;
			_objectServer = new ObjectServer(_endPointName);

			this.D("{0} serving".format(endPointName));
		}

		public void Dispose()
		{
			_objectServer.Dispose();
			_mutex.Close();
		}

		public IObjectEndPoint tryAccept(uint? timeout, WaitHandle interrupt_)
		{
			return _objectServer.tryAccept(timeout, interrupt_);
		}
	}
}
