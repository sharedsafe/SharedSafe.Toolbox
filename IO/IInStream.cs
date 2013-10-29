using System;
using System.IO;

namespace Toolbox.IO
{
	public interface IInStream
	{
		/// Reads a number of bytes from the data stream.
		/// Returns less number of bytes than requested, if the stream has ended.

		uint readBytes(byte[] array, uint offset, uint length);
	}

	public static partial class InStreamExtensions
	{
		#region IInStream

		const uint BufSize = 0x10000;

		public static ulong copyTo(this IInStream input, Stream output)
		{
			return copyTo(input, output.toOutStream());
		}

		public static ulong copyTo(this IInStream input, IOutStream output)
		{
			var buf = new byte[BufSize];

			ulong copied = 0;

			while(true)
			{
				uint bytes = input.readBytes(buf, 0, BufSize);
				if (bytes == 0)
					return copied;

				output.writeBytes(buf, 0, bytes);
				copied += bytes;
			}
		}

		/// For optimization, the returned buffer may be larger than the number of bytes receive!

		// todo: replace this with BufferReference!

		public static Pair<byte[], uint> toBoundArray(this IInStream input)
		{
			// todo: what about a bound memory out stream?
			using (var ms = new MemoryStream())
			{
				ulong len = input.copyTo(ms);

				// this is funny here, but don't know right now what to do... copyTo() is expected to 
				// be able to process more than 4 gigs, but toBoundArray() of course is not.

				if (len > uint.MaxValue)
					throw new OutOfMemoryException("data stream too long");

				return Pair.make(ms.GetBuffer(), (uint) len);
			}
		}

		public static byte[] toArray(this IInStream input)
		{
			var bound = input.toBoundArray();
			if (bound.First.Length == bound.Second)
				return bound.First;

			var copied = new byte[bound.Second];
			Array.Copy(bound.First, 0, copied, 0, copied.Length);
			return copied;
		}

		#endregion

		#region Stream extensions

		public static IInStream toInStream(this Stream stream)
		{
			return new StreamInStream(stream);
		}

		sealed class StreamInStream : IInStream, IBoundStream
		{
			readonly Stream _stream;

			public StreamInStream(Stream stream)
			{
				_stream = stream;
			}

			#region IInStream Members

			public uint readBytes(byte[] array, uint offset, uint length)
			{
				return (uint)_stream.Read(array, (int)offset, (int)length);
			}

			#endregion

			#region IBoundStream Members

			public ulong Length
			{
				get { return (ulong)(_stream.Length); }
			}

			public ulong BytesLeft
			{
				get { return (ulong)(_stream.Length - _stream.Position); }
			}

			#endregion
		}

		#endregion

		#region byte[] extensions

		public static IInStream toInStream(this byte[] bytes)
		{
			return new ByteInStream(bytes);
		}

		sealed class ByteInStream : IInStream
		{
			readonly byte[] _bytes;
			ulong _offset;

			public ByteInStream(byte[] bytes)
			{
				_bytes = bytes;
			}

			#region IInStream Members

			public uint readBytes(byte[] array, uint offset, uint length)
			{
				ulong toCopy = Math.Min((ulong)_bytes.LongLength - _offset, length);
				if (toCopy == 0)
					return 0;

				Array.Copy(_bytes, (long)_offset, array, offset, (long)toCopy);
				_offset += toCopy;

				return (uint)toCopy;
			}

			#endregion
		}

		#endregion

		
	}
}
