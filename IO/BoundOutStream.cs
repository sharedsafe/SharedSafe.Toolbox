/**
	Write a maximum number of bytes to an IOutStream.

	If more bytes are written, an IOException is thrown. Writing less bytes is fine. 
	Clients that create a BoundOutStream may test BytesWritten if the number of written 
	bytes exactly matches the bytes expected.
**/

using System.IO;

namespace Toolbox.IO
{
	public interface IBoundOutStream : IOutStream, IBoundStream
	{
		ulong BytesWritten { get; }
	}

	sealed class BoundOutStream : IBoundOutStream
	{
		readonly IOutStream _stream;
		readonly ulong _length;

		public ulong BytesWritten { get; private set; }

		public BoundOutStream(IOutStream stream, ulong length)
		{
			_stream = stream;
			_length = length;
		}


		#region IOutStream Members

		public void writeBytes(byte[] array, uint offset, uint length)
		{
			if (length + BytesWritten > _length)
				throw new IOException("Tried to write too much data to a bound out stream of length {0}".format(_length));

			_stream.writeBytes(array, offset, length);
			BytesWritten += length;
		}

		public ulong Length
		{
			get 
			{
				return _length;
			}
		}

		public ulong BytesLeft
		{
			get 
			{
				return _length - BytesWritten;
			}
		}

		#endregion
	}

	public static partial class OutStreamExtensions
	{
		public static IBoundOutStream bound(this IOutStream stream, ulong length)
		{
			return new BoundOutStream(stream, length);
		}
	}
}
