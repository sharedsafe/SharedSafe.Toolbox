//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeSocket.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace Toolbox.IPC
{
	/// <summary>
	/// Implements a local Named Pipe socket.
	/// </summary>
	sealed class NamedPipeSocket : IDisposable
	{
		IntPtr _pipe;

		/// <summary>
		/// Creates a new socket instance form the specified local Named Pipe handle.
		/// </summary>
		/// <param name="hPipe">The handle.</param>
		internal NamedPipeSocket(IntPtr hPipe)
		{
			_pipe = hPipe;
			Info = new NamedPipeSocketInfo(hPipe);
		}

		~NamedPipeSocket() 
		{
			Dispose();
		}

		/// <summary>
		/// Gets the NamedPipeSocketInfo of this instance.
		/// </summary>
		/// <returns></returns>
		public NamedPipeSocketInfo Info { get; private set; }


		/// <summary>
		/// Closes the socket.
		/// </summary>
		public void Close() 
		{
			Dispose();
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		
		// asander: made this public, so that using() constructs work
		public void Dispose()
		{
			if (_pipe == IntPtr.Zero)
				return;
			try 
			{
				// disconnect the pipe
				if (Info.IsServer) 
				{
					NamedPipeHelper.FlushFileBuffers(_pipe);
					NamedPipeHelper.DisconnectNamedPipe(_pipe);
				}
			}
			catch (NamedPipeException) 
			{
			}

			NamedPipeHelper.CloseHandle(_pipe);
			_pipe = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Returns the stream used to send and receive data.
		/// </summary>
		/// <returns></returns>
		public Stream GetStream() 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);

			return stream ?? (stream = new NamedPipeStream(this, false));
		}

		Stream stream;

		/// <summary>
		/// Returns the stream used to send and receive data. The stream disposes
		/// the socket on close.
		/// </summary>
		/// <returns></returns>
		public Stream GetClosingStream() 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);
			
			return new NamedPipeStream(this, true);
		}

		/// <summary>
		/// Flushes the socket.
		/// </summary>
		public void Flush() 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);

			NamedPipeHelper.FlushFileBuffers(_pipe);
		}

		/// <summary>
		/// Receives the specified number of bytes from a socket into
		/// the specified offset position of the receive buffer.
		/// </summary>
		/// <param name="buffer">An array of type Byte that is the storage
		/// location for received data.</param>
		/// <param name="offset">The location in buffer to store the received data.</param>
		/// <param name="count">The number of bytes to receive.</param>
		/// <returns>The number of bytes received.</returns>

		public uint Receive(byte[] buffer, uint offset, uint count)
		{
			return Receive(buffer, offset, count, null, null).First;
		}

		public Pair<uint, bool> Receive(byte[] buffer, uint offset, uint count, uint? timeout, WaitHandle interrupt_) 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			// client must use returned count==0 to find out about disconnects
			if (count == 0)
				throw new ArgumentException("count");
			if (buffer.Length < offset + count)
				throw new ArgumentException();


			// todo: don't use overlapped if this is not a overlapped pipe
			// (note: overlapped pipes can be read and written to from different
			// threads, which is something we sometimes need in IPC)
			// note: non-overlapped use of ReadFile conflicts with overlapped write
			// from different threads (see documentation, ReadFile reports incorrectly number
			// of read bytes and returns too early), so we always use overlapped reads here!

			using (var overlapped = OverlappedEvent.use())
			{
				var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				try
				{
					// todo: there is a problem with using stacked NativeOverlapped
					// (can be shown when all throws of the debugging assistants are enabled)

					uint read;
					bool res = NamedPipeHelper.ReadFile(
						_pipe,
						Marshal.UnsafeAddrOfPinnedArrayElement(buffer, (int) offset),
						count,
						out read,
						overlapped.NativeIntPtr
						);
					

					if (res)
					{
						this.T("immediate read: " + read);
						// read == 0 indicates end of pipe.
						Debug.Assert(read != 0);
						return Pair.make(read, false);
					}

					var lastError = Marshal.GetLastWin32Error();
					if (lastError != NamedPipeHelper.ERROR_IO_PENDING)
					{
						if (
							lastError == NamedPipeHelper.ERROR_BROKEN_PIPE || 
							lastError == NamedPipeHelper.ERROR_PIPE_NOT_CONNECTED)
							return Pair.make(0u, false);
						throw new NamedPipeException();
					}

					bool seenTimeoutOrInterrupt = false;

					if (!overlapped.wait(timeout, interrupt_))
					{
						// may be finished if CancelIo fails?
						if (!NamedPipeHelper.CancelIo(_pipe))
							this.E("Canceling IO failed");

						seenTimeoutOrInterrupt = true;
					}

					if (!NamedPipeHelper.GetOverlappedResult(_pipe, overlapped.NativeIntPtr, out read, seenTimeoutOrInterrupt))
					{
						lastError = Marshal.GetLastWin32Error();
						if (lastError != NamedPipeHelper.ERROR_OPERATION_ABORTED)
						{
							if (lastError == NamedPipeHelper.ERROR_BROKEN_PIPE || 
								lastError == NamedPipeHelper.ERROR_PIPE_NOT_CONNECTED)
								return Pair.make(0u, false);
							throw new NamedPipeException();
						}
					}

					this.T("inner read: " + read + " (overlapped)");
					// read == 0 indicates end of pipe, so we don't accept this here, overlapped or not.
					Debug.Assert(seenTimeoutOrInterrupt || read != 0);
					return Pair.make(read, seenTimeoutOrInterrupt);
				}
				finally
				{
					gch.Free();
				}
			}
		}

		delegate uint ReceiveMethod(byte[] buffer, uint offset, uint count);

		public IAsyncResult BeginReceive(byte[] buffer, uint offset, uint count,
			AsyncCallback callback, object state)
		{
			return new ReceiveMethod(Receive).BeginInvoke(buffer, offset, count, callback, state);
		}

// ReSharper disable MemberCanBeMadeStatic
		public uint EndReceive(IAsyncResult asyncResult) 
// ReSharper restore MemberCanBeMadeStatic
		{
			var ar = (AsyncResult)asyncResult;
			return ((ReceiveMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
		}

		/// <summary>
		/// Sends the specified number of bytes of data to a connected socket,
		/// starting at the specified offset.
		/// </summary>
		/// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
		/// <param name="offset">The position in the data buffer at which to begin sending data. </param>
		/// <param name="count">The number of bytes to send.</param>
		/// <returns>The number of bytes sent.</returns>
		public uint Send(byte[] buffer, uint offset, uint count) 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (buffer.Length < offset + count)
				throw new ArgumentException();

			GCHandle gch  = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try 
			{
				uint written;
				bool res = NamedPipeHelper.WriteFile(
					_pipe,
					Marshal.UnsafeAddrOfPinnedArrayElement(buffer, (int)offset),
					count,
					out written,
					IntPtr.Zero
					);

				if (!res) throw new NamedPipeException();
				return written;
			}
			finally 
			{
				gch.Free();
			}
		}

		public void SendAll(byte[] buffer, uint offset, uint count)
		{
			while (count != 0)
			{
				uint sent = Send(buffer, offset, count);
				if (sent == 0)
					throw new NamedPipeException();

				count -= sent;
				offset += sent;
			}
		}

		delegate uint SendMethod(byte[] buffer, uint offset, uint count);

		public IAsyncResult BeginSend(byte[] buffer, uint offset, uint count,
			AsyncCallback callback, object state)
		{
			return new SendMethod(Send).BeginInvoke(buffer, offset, count, callback, state);
		}

// ReSharper disable MemberCanBeMadeStatic
		public uint EndSend(IAsyncResult asyncResult) 
// ReSharper restore MemberCanBeMadeStatic
		{
			var ar = asyncResult as AsyncResult;
// ReSharper disable PossibleNullReferenceException
			return ((SendMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
// ReSharper restore PossibleNullReferenceException
		}

		/// <summary>
		/// Returns the current NamedPipeSocketState of this instance.
		/// </summary>
		/// <returns></returns>
		public NamedPipeSocketState GetSocketState() 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);

			return new NamedPipeSocketState(_pipe);
		}

		/// <summary>
		/// Impersonates the client.
		/// </summary>
		public void Impersonate() 
		{
			if (_pipe == IntPtr.Zero)
				throw new ObjectDisposedException(GetType().FullName);

			if (Info.IsServer)
				if (!NamedPipeHelper.ImpersonateNamedPipeClient(_pipe))
					throw new NamedPipeException();
		}

		/// <summary>
		/// Reverts the impersonation.
		/// </summary>
		public static bool RevertToSelf() 
		{
			return NamedPipeHelper.RevertToSelf();
		}

	}

	/// <summary>
	/// Represents local Named Pipe informations.
	/// </summary>
	public sealed class NamedPipeSocketInfo
	{
		public readonly int Flags;
		public readonly int OutBufferSize;
		public readonly int InBufferSize;
		public readonly int MaxInstances;

		public bool IsServer 
		{
			get 
			{
				return (Flags & NamedPipeHelper.PIPE_SERVER_END) != 0;
			}
		}

		internal NamedPipeSocketInfo(IntPtr hPipe) 
		{
			bool res = NamedPipeHelper.GetNamedPipeInfo(
				hPipe,
				out Flags,
				out OutBufferSize,
				out InBufferSize,
				out MaxInstances
				);
			
			if (!res) 
			{
				throw new NamedPipeException();
			}
		}
	}

	/// <summary>
	/// Represents local Named Pipe state informations.
	/// </summary>
	public sealed class NamedPipeSocketState 
	{
		public readonly int State;
		public readonly int CurrentInstances;
		public readonly int MaxCollectionCount;
		public readonly int CollectDataTimeout;
		public readonly string UserName;

		internal NamedPipeSocketState(IntPtr hPipe) 
		{
			var userName = new StringBuilder(256);
			bool res = NamedPipeHelper.GetNamedPipeHandleState(
				hPipe,
				out State,
				out CurrentInstances,
				out MaxCollectionCount,
				out CollectDataTimeout,
				userName,
				userName.Capacity
				);

			if (!res)				throw new NamedPipeException();
			UserName = userName.ToString();
		}
	}
}
