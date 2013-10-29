using System;

namespace Toolbox.IO
{
	public interface IBoundInStream : IInStream, IBoundStream
	{
		ulong BytesRead { get; }
	}
	
	sealed class BoundInStream : IBoundInStream
	{
		readonly IInStream _stream;
		readonly ulong _length;

		public BoundInStream(IInStream stream, ulong length)
		{
			_stream = stream;
			_length = length;
		}

		#region IInStream Members

		public uint readBytes(byte[] array, uint offset, uint length)
		{
			ulong maxToRead = _length - BytesRead;
			uint read = _stream.readBytes(array, offset, (uint)Math.Min(length, maxToRead));
			BytesRead += read;
			return read;
		}

		public ulong BytesRead 
		{ get; private set; }

		public ulong Length
		{ get { return _length; } }

		public ulong BytesLeft
		{ get { return _length - BytesRead; } }

		#endregion
	}

	public static partial class InStreamExtensions
	{
		public static IBoundInStream bound(this IInStream stream, ulong length)
		{
			return new BoundInStream(stream, length);
		}
	}
}
