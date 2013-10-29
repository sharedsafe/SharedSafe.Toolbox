using System.IO;
using Toolbox.IO;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	sealed class NativeFileStream : IAcquiredStream, IBoundStream
	{
		readonly IInStream _stream;
		readonly Stream _sysStream;

		public NativeFileStream(Stream stream)
		{
			_sysStream = stream;
			_stream = stream.toInStream();
		}

		#region IInStream Members

		public uint readBytes(byte[] array, uint offset, uint length)
		{
			return _stream.readBytes(array, offset, length);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_sysStream.Dispose();
		}

		#endregion

		#region IBoundStream Members

		public ulong Length
		{
			get { return ((IBoundStream) _stream).Length; }
		}

		public ulong BytesLeft
		{
			get
			{
				return ((IBoundStream) _stream).BytesLeft;
			} 
		}

		#endregion
	}
}