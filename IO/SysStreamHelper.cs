using System;
using System.IO;

namespace Toolbox.IO
{
	public static class SysStreamHelper
	{
		public static Stream toSysStream(this IInStream stream)
		{
			return new SysInStream(stream);
		}

		sealed class SysInStream : Stream
		{
			ulong _offset;
			ulong? _length;
			readonly IInStream _stream;

			public SysInStream(IInStream stream)
			{
				_stream = stream;
				var bound = stream as IBoundStream;
				if (bound != null)
					_length = bound.Length;
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
				get { return false; }
			}

			public override void Flush()
			{
			}

			public override long Length
			{
				get
				{
					if (_length == null)
						throw new NotImplementedException();
					return (long)_length.Value;
				}
			}

			public override long Position
			{
				get
				{
					return (long)_offset;
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				uint read = _stream.readBytes(buffer, (uint)offset, (uint)count);
				_offset += read;
				return (int)read;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException();
			}
		}

	}
}
