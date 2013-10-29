//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeClient.cs
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
using System.Runtime.InteropServices;

namespace Toolbox.IPC
{
	/// <summary>
	/// Provides client connections for local Named Pipes.
	/// </summary>
	static class NamedPipeClient
	{
		/// Try to connect to a pipe, return null if it is not available or the timeout expired.

		public static NamedPipeSocket tryConnect(string name)
		{
			string pipeName = NamedPipeHelper.formatPipeName(name);

			while (true)
			{

				IntPtr hPipe = NamedPipeHelper.CreateFile(
					pipeName,
					NamedPipeHelper.GENERIC_READ |
						NamedPipeHelper.GENERIC_WRITE,
					0,
					IntPtr.Zero,
					NamedPipeHelper.OPEN_EXISTING,
					NamedPipeHelper.FILE_FLAG_OVERLAPPED,
					IntPtr.Zero
					);


				if (hPipe.ToInt32() != NamedPipeHelper.INVALID_HANDLE_VALUE)
					return new NamedPipeSocket(hPipe);

				int lastError = Marshal.GetLastWin32Error();
				if (lastError != NamedPipeHelper.ERROR_PIPE_BUSY)
					return null; // pipe does not exist.

				// we do also wait for the pipe if the file is not found.

				if (!NamedPipeHelper.WaitNamedPipe(pipeName, (int)NamedPipeHelper.NMPWAIT_USE_DEFAULT_WAIT))
					return null; // timeout, server is not responsive.
			}
		}

#if false
		/// <summary>
		/// Connects to a local Named Pipe server.
		/// </summary>
		/// <param name="name"> The name of the pipe to connect to </param>
		/// <param name="timeout">Timeout in millisecons to wait for the connection. 0 to return null in case the pipe cannot connect.</param>
		/// <returns></returns>
		
		public static NamedPipeSocket Connect(string name, int timeout)
		{
			string pipeName = NamedPipeHelper.FormatPipeName(name);

			while (true)
			{
				IntPtr hPipe = NamedPipeHelper.CreateFile(
					pipeName,
					NamedPipeHelper.GENERIC_READ |
					NamedPipeHelper.GENERIC_WRITE,
					0,
					IntPtr.Zero,
					NamedPipeHelper.OPEN_EXISTING,
					0,
					IntPtr.Zero
					);

				if (hPipe.ToInt32() != NamedPipeHelper.INVALID_HANDLE_VALUE)
					return new NamedPipeSocket(hPipe);

				int lastError = Marshal.GetLastWin32Error();
				if (lastError != NamedPipeHelper.ERROR_PIPE_BUSY)
					throw new NamedPipeException(lastError);

				if (!NamedPipeHelper.WaitNamedPipe(pipeName, timeout))
					throw new NamedPipeException();
			}
		}

		/// Try to connect to a pipe, return null if it is not available!
		
		public static NamedPipeSocket TryConnect(string name, int timeout)
		{
			var socket = TryConnect(name);
			if (socket != null)
				return socket;

			int lastError = Marshal.GetLastWin32Error();
			if (lastError != NamedPipeHelper.ERROR_PIPE_BUSY || timeout == 0)
				return null;

			string pipeName = NamedPipeHelper.FormatPipeName(name);

			if (!NamedPipeHelper.WaitNamedPipe(pipeName, timeout))
				return null;

			return TryConnect(name);
		}

#endif
	}
}
