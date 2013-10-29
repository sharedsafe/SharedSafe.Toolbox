using System;
using System.Diagnostics;
using System.Threading;

namespace Toolbox.IPC
{
	public interface IObjectReceiver
	{
		object tryReceive(Type type, uint? timeout, WaitHandle interrupt_);
		bool EOF { get; }
	}

	public interface IObjectTransmitter
	{
		// Note; the type needs to be passed here to support casting the obj to a derived type object extends type.
		void send(Type type, object obj);
	}

	public interface IObjectEndPoint : IObjectReceiver, IObjectTransmitter, IDisposable
	{
	}

	public static class ObjectEndPointExtensions
	{
		#region Receive, Typed

		public static T receive<T>(this IObjectReceiver ep)
		{
			return (T)ep.receive(typeof(T));
		}

		#endregion

		#region TryReceive, Typed

		public static T tryReceive<T>(this IObjectReceiver ep, uint? timeout)
			where T : class
		{
			var obj = ep.tryReceive(typeof(T), timeout, null);
			return obj as T;
		}

		public static T tryReceive<T>(this IObjectReceiver ep, WaitHandle interrupt_)
			where T : class
		{
			var obj = ep.tryReceive(typeof(T), null, interrupt_);
			return obj as T;
		}

		#endregion

		public static object receive(this IObjectReceiver ep, Type type)
		{
			var obj = ep.tryReceive(type, null, null);
			if (obj == null)
			{
				Debug.Assert(ep.EOF);
				throw new Exception("Receiving of object failed: Pipe closed.");
			}

			return obj;
		}

		public static void send<T>(this IObjectTransmitter ep, T obj)
		{
			ep.send(typeof(T), obj);
		}

		public static ResponseT sendReceive<RequestT, ResponseT>(this IObjectEndPoint ep, RequestT request)
		{
			ep.send(typeof(RequestT), request);
			return ep.receive<ResponseT>();
		}

		public static IAsynchronousObjectTransmitter createAsynchronousSender(this IObjectTransmitter ep)
		{
			return new AsynchronousObjectTransmitter(ep);
		}
	}
}
