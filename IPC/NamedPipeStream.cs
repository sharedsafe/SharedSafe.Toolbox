//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeStream.cs
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
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace Toolbox.IPC
{
	/// <summary>
	/// Provides the underlying stream of data for local Named Pipe access.
	/// </summary>
	class NamedPipeStream : Stream
	{
		readonly NamedPipeSocket _socket;
		readonly bool _ownsSocket;

		/// <summary>
		/// Creates a new instance of the NamedPipeStream class for the specified NamedPipeSocket.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="ownsSocket"></param>
		public NamedPipeStream(NamedPipeSocket socket, bool ownsSocket)
		{
			_socket = socket;
			_ownsSocket = ownsSocket;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Close() 
		{
			if (_ownsSocket) 
				_socket.Close();
		}

		public override void Flush() 
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return (int)_socket.Receive(buffer, (uint)offset, (uint)count);
		}

		delegate int ReadMethod(byte[] buffer, int offset, int count);

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count,
			AsyncCallback callback, object state)
		{
			return new ReadMethod(Read).BeginInvoke(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			var ar = asyncResult as AsyncResult;
// ReSharper disable PossibleNullReferenceException
			return ((ReadMethod)ar.AsyncDelegate).EndInvoke(ar);
// ReSharper restore PossibleNullReferenceException
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_socket.SendAll(buffer, offset.unsigned(), count.unsigned());
		}
		
		delegate void WriteMethod(byte[] buffer, int offset, int count);

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count,
			AsyncCallback callback, object state)
		{
			return new WriteMethod(Write).BeginInvoke(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			var ar = asyncResult as AsyncResult;
// ReSharper disable PossibleNullReferenceException
			((WriteMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
// ReSharper restore PossibleNullReferenceException
		}

	}
}