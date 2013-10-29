using System;
using System.Diagnostics;

namespace Toolbox.IO
{
	public sealed class MemoryInStream : IBoundInStream
	{
		readonly byte[] _data;
		readonly uint _begin;
		readonly uint _end;
		uint _offset;
		
		public MemoryInStream(byte[] data)
			: this(data, 0, (uint)data.Length)
		{
		}
	
		public MemoryInStream(byte[] data, uint offset, uint length)
		{
			Debug.Assert(offset + length <= data.Length);

			_data = data;
			_begin = offset;
			_end = offset + length;
			_offset = offset;
		}


		#region IInStream Members

		public uint readBytes(byte[] array, uint offset, uint length)
		{
			Debug.Assert(offset + length <= array.Length);

			uint toRead = Math.Min(length, _end - _offset);
			Array.Copy(_data, _offset, array, offset, toRead);
			_offset += toRead;
			return toRead;
		}

		public ulong Length
		{
			get { return _end - _begin; }
		}

		public ulong BytesLeft
		{
			get { return _end - _offset; }
		}

		public ulong BytesRead
		{
			get { return _offset - _begin; }
		}

		#endregion
	}
}
