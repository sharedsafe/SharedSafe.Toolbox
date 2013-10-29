using System.Threading;

namespace Toolbox.IPC
{
	sealed class ObjectServer : IObjectServer
	{
		readonly NamedPipeListener _listener;

		public string Name { get; private set; }

		public ObjectServer(string name)
			: this(name, false)
		{
		}

		public ObjectServer(string name, bool global)
		{
			Name = name;
			_listener = new NamedPipeListener(ObjectHelper.makePipeName(name, global));
		}

		public void Dispose()
		{
			_listener.Dispose();
		}

		public IObjectEndPoint tryAccept(uint? timeout, WaitHandle interrupt_)
		{
			var socket = _listener.Accept(timeout, interrupt_);
			return socket == null ? null : new ObjectEndPoint(socket);
		}
	}
}
