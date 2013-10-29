using System;
using System.Diagnostics;

namespace Toolbox.IO
{
	sealed class BufferedInStream : IInStream
	{
		readonly IInStream _stream;
		readonly byte[] _buf;
		uint _offset;
		uint _end;

		public BufferedInStream(IInStream stream, uint buffer)
		{
			_stream = stream;
			_buf = new byte[buffer];
		}
			
		#region IInStream Members

		public uint readBytes(byte[] array, uint offset, uint length)
		{
			uint end = offset + length;
			Debug.Assert(end <= array.Length);

			while (offset != end)
			{
				if (_offset == _end)
					if (!readMoreData())
						return length - (end - offset);

				uint now = Math.Min(end - offset, _end - offset);
				Array.Copy(_buf, _offset, array, offset, now);
				offset += now;
				_offset += now;
			}

			return length;
		}

		bool readMoreData()
		{
			Debug.Assert(_offset == _end);
			_end = _stream.readBytes(_buf, 0, (uint)_buf.Length);
			_offset = 0;
			return _end != 0;
		}

		#endregion
	}
}
