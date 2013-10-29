// #define DEBUG_IPC

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using JsonExSerializer;
using Toolbox.Serialization;

namespace Toolbox.IPC
{
	sealed class ObjectEndPoint : IObjectEndPoint
	{
		readonly NamedPipeSocket _socket;
		// note that sends and receive can be done in parallel (from different threads)
		Serializer _receiveSerializer;
		Serializer _sendSerializer;

		readonly IEnumerator<string> _packets;
		uint? _currentReceiveTimeout;
		WaitHandle _currentReceiveInterrupt_;

		public bool EOF { get; private set; }

		internal ObjectEndPoint(NamedPipeSocket socket)
		{
			_socket = socket;
			_packets = readAndDecodePackets().GetEnumerator();
		}
		
		public static ObjectEndPoint tryConnect(string remotePoint)
		{
			return tryConnect(remotePoint, false);
		}


		public static ObjectEndPoint tryConnect(string remotePoint, bool global)
		{
			var client = NamedPipeClient.tryConnect(ObjectHelper.makePipeName(remotePoint, global));
			return client == null ? null : new ObjectEndPoint(client);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_socket.Close();
		}

		#endregion

#if false
		// no null checking here? removed this function for now.

		public object receive(Type type)
		{
			return tryReceive(type, null, null);
		}
#endif

		/**
			null => either timeout, interrupt or EOF, 
			[clients need to check EOF property for discrimination]
		**/

		public object tryReceive(Type type, uint? timeout, WaitHandle interrupt_)
		{
#if DEBUG_IPC
			this.D("recv in : " + type.Name);
#endif

			_currentReceiveTimeout = timeout;
			_currentReceiveInterrupt_ = interrupt_;

			bool nextPacketAvailable = _packets.MoveNext();
			if (!nextPacketAvailable)
			{
				EOF = true;
				this.T("recv out: EOF");
				return null;
			}

			string data = _packets.Current;
			if (data == null)
			{
				this.T("recv out: timeout");
				return null;
			}

			refreshSerializer(ref _receiveSerializer, type);

			var obj = _receiveSerializer.Deserialize(data);

#if DEBUG_IPC
			this.D("recv out: " + obj);
#endif

			return obj;
		}

		public void send(Type type, object obj)
		{
#if DEBUG_IPC
			this.D("send in : " + type.Name);
#endif

			string serialized = serialize(type, obj);
			sendAsPacket(Encoding.UTF8.GetBytes(serialized));

#if DEBUG_IPC
			this.D("send out: " + obj);
#endif
		}

		string serialize(Type type, object obj)
		{
			var builder = new StringBuilder();
			var stringWriter = new StringWriter(builder);
			refreshSerializer(ref _sendSerializer, type);
			_sendSerializer.Serialize(obj, stringWriter);
			return builder.ToString();
		}

		void sendAsPacket(byte[] encoded)
		{
			_socket.SendAll(encoded, 0, encoded.Length.unsigned());
			_socket.SendAll(PacketTerminator, 0, PacketTerminator.Length.unsigned());
			// must flush to prevent Dispose() from killing data.
			_socket.Flush();
		}

		static void refreshSerializer(ref Serializer serializer, Type type)
		{
			if (serializer != null && type.Equals(serializer.SerializedType))
				return;

			// new: use context from Toolbox.Serialization
			serializer = new Serializer(type, type.serializationContext());
		}

		static readonly byte[] PacketTerminator = new byte[] {0};

		IEnumerable<string> readAndDecodePackets()
		{
#if DEBUG
			const uint ObjectBufferInitialSize = 4;
#else
			const uint ObjectBufferInitialSize = NamedPipeListener.DefaultBufferSize;
#endif

			var objectBuffer = new byte[ObjectBufferInitialSize];
			uint count = 0;

			while (true)
			{
				uint newMaxCount = count + NamedPipeListener.DefaultBufferSize;

				ensureCapacity(ref objectBuffer, count, newMaxCount);

				Pair<uint, bool> readT = 
					_socket.Receive(objectBuffer, count, NamedPipeListener.DefaultBufferSize, _currentReceiveTimeout, _currentReceiveInterrupt_);

				var read = readT.First;
				var seenInterruptOrTimeout = readT.Second;

				if (!seenInterruptOrTimeout && read == 0)
				{
					this.T("No bytes to receive from pipe, remote end may have closed the connection");
					yield break;
				}

				this.T("read: " + read);

				uint newCount = count + read;
				uint consumed = 0;

				for (; count != newCount; ++count)
				{
					byte b = objectBuffer[count];
					if (b != 0)
						continue;

					yield return Encoding.UTF8.GetString(objectBuffer, (int)consumed, (int) (count - consumed));
					consumed = count + 1;
				}

				Debug.Assert(count == newCount);
				if (consumed != 0 && count != consumed)
					Array.Copy(objectBuffer, consumed, objectBuffer, 0, count - consumed);
				count -= consumed;

				// all bytes processed, notify of timeout?
				if (seenInterruptOrTimeout)
					yield return null;
			}
		}

		static void ensureCapacity(ref byte[] objectBuffer, uint count, uint newMaxCount)
		{
			uint newCapacity = objectBuffer.Length.unsigned();
			while (newCapacity < newMaxCount)
				newCapacity <<= 1;

			if (newCapacity == objectBuffer.Length)
				return;

			var newArray = new byte[newCapacity];
			Array.Copy(objectBuffer, newArray, count);
			objectBuffer = newArray;
		}
	}
}

