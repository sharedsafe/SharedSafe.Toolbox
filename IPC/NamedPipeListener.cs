//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeListener.cs
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
using System.Runtime.InteropServices;
using System.Threading;

namespace Toolbox.IPC
{
	/// <summary>
	/// Listens for connections from local local Named Pipe clients.
	/// </summary>
	sealed class NamedPipeListener : IDisposable
	{
		public const uint DefaultBufferSize = 4096;
		public const uint DefaultConnectionTimeout = 10000;

		public uint BufferSize { get; private set; }
		public string Name { get; private set; }

		IntPtr _pipeHandle;		/// <summary>
		/// Creates a new listener with the specified name.
		/// </summary>
		/// <param name="pipeName">The pipe name omiting the leading <c>\\.\pipe\</c></param>

		public NamedPipeListener(string pipeName)
			: this(pipeName, DefaultBufferSize)
		{
		}

		public void Dispose()		{
			destroyPipe();
		}

		public NamedPipeListener(string pipeName, uint bufferSize)
		{
			Name = NamedPipeHelper.formatPipeName(pipeName);
			BufferSize = bufferSize;
		
			_pipeHandle = new IntPtr(NamedPipeHelper.INVALID_HANDLE_VALUE); 
		}

		/// <summary>
		/// Accepts a pending connection request
		/// </summary>
		/// <remarks>
		/// Accept is a blocking method that returns a NamedPipeSocket you can use to send and receive data. 
		/// </remarks>
		/// <returns>The NamedPipeSocket.</returns>
		
		public NamedPipeSocket Accept()		{
			return Accept(null);
		}

		public NamedPipeSocket Accept(uint? timeout)
		{
			return Accept(timeout, null);
		}
		
		public NamedPipeSocket Accept(uint? timeout, WaitHandle interrupt_) 
		{
			// note: from now on, clients can connect to the pipe.

			bool isConnected;
			int lastError = 0;
			bool aborted = false;

			var thisPipe = takePipe();

			using (var overlapped = OverlappedEvent.use())
			{
				// this.D("Accepting (connecting to named pipe)");

				while(true)
				{
					isConnected = NamedPipeHelper.ConnectNamedPipe(thisPipe, overlapped.NativeIntPtr);
					if (isConnected)
						break;

					lastError = Marshal.GetLastWin32Error();
					if (lastError != NamedPipeHelper.ERROR_NO_DATA)
						break;

					this.D("No data, trying again");

					var nextPipe = takePipe();
					closePipeHandle(thisPipe);
					thisPipe = nextPipe;
				}

				if (!isConnected)
				{
					switch (lastError)
					{
						
						case NamedPipeHelper.ERROR_PIPE_CONNECTED:
							isConnected = true;
							break;

						case NamedPipeHelper.ERROR_IO_PENDING:
							bool timeoutOrInterrupt = false;

							if (!overlapped.wait(timeout, interrupt_))
							{
								// hmm, closehandle works here, no need to cancel it seems.
								timeoutOrInterrupt = true;
								if (!NamedPipeHelper.CancelIo(thisPipe))
									this.D("Failed to cancel IO");
							}

							uint dummy;
							if (!NamedPipeHelper.GetOverlappedResult(thisPipe, overlapped.NativeIntPtr, out dummy, timeoutOrInterrupt))
							{
								lastError = Marshal.GetLastWin32Error();
								aborted = timeoutOrInterrupt && lastError == NamedPipeHelper.ERROR_OPERATION_ABORTED;
								if (!aborted)
									this.D("Getting overlapped result failed: " + lastError);
							}
							else
								isConnected = true;

							break;

						default:
							this.D("ConnectNamedPipe failed: " + lastError);
							break;
					}
				}
			}


			if (!isConnected)
			{
				reusePipe(thisPipe);
				if (aborted)
					return null;
				throw new NamedPipeException(lastError);
			}

			// before leaving, we need to create a new pipe. If clients close the returned 
			// one all handles to the pipe for this process are closed and connections 
			// that happened between that time are rejected.

			_pipeHandle = createNewPipe();
			return new NamedPipeSocket(thisPipe);
		}

		static void closePipeHandle(IntPtr thisPipe)
		{
			// this.D("Closing pipe handle " + thisPipe.ToInt32());
			NamedPipeHelper.CloseHandle(thisPipe);
		}

		void reusePipe(IntPtr thisPipe)
		{
			Debug.Assert(_pipeHandle.ToInt32() == NamedPipeHelper.INVALID_HANDLE_VALUE);
			_pipeHandle = thisPipe;
		}

		IntPtr takePipe()
		{
			if (_pipeHandle.ToInt32() == NamedPipeHelper.INVALID_HANDLE_VALUE)
				return createNewPipe();

			var handle = _pipeHandle;
			_pipeHandle = new IntPtr(NamedPipeHelper.INVALID_HANDLE_VALUE);
			return handle;
		}

		// note: before closing any handle, we must be sure to create a new instance of the pipe, if 
		// there is a situation with all handles closed, clients may not be able to connect anymore :(
		
		IntPtr createNewPipe()
		{
			var newPipe = NamedPipeHelper.CreateNamedPipe(
				Name,
				NamedPipeHelper.PIPE_ACCESS_DUPLEX
					// | (timeout != null ? NamedPipeHelper.FILE_FLAG_OVERLAPPED : 0)
					// changed to always overlapped, to support simultaneous reads and writes.
					| NamedPipeHelper.FILE_FLAG_OVERLAPPED
				,
				NamedPipeHelper.PIPE_TYPE_BYTE | NamedPipeHelper.PIPE_READMODE_BYTE
					// | NamedPipeHelper.PIPE_READMODE_MESSAGE
					| NamedPipeHelper.PIPE_WAIT
				,
				NamedPipeHelper.PIPE_UNLIMITED_INSTANCES,
				BufferSize,
				BufferSize,
				DefaultConnectionTimeout,
				IntPtr.Zero
				);

			if (newPipe.ToInt32() == NamedPipeHelper.INVALID_HANDLE_VALUE)
				throw new NamedPipeException();

			this.D("Created new pipe");
		

			return newPipe;
		}

		void destroyPipe()
		{
			if (_pipeHandle.ToInt32() == NamedPipeHelper.INVALID_HANDLE_VALUE)
				return;

			this.D("Closing pipe handle " + _pipeHandle.ToInt32());
			NamedPipeHelper.CloseHandle(_pipeHandle);
			_pipeHandle = new IntPtr(NamedPipeHelper.INVALID_HANDLE_VALUE);
		}
	}
}