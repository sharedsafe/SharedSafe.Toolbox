using System;
using System.Diagnostics;
using System.IO;

namespace Toolbox.IO
{
	public struct BufferReference
	{
		internal BufferReference(byte[] buffer)
			: this(buffer, 0, buffer.Length)
		{
		}

		internal BufferReference(byte[] buffer, int offset, int length)
		{
			Buffer = buffer;
			Offset = offset;
			Length = length;
		}

		public readonly byte[] Buffer;
		public readonly int Offset;
		public readonly int Length;

		public byte this [int offset]
		{
			get
			{
				Debug.Assert(offset < Length);
				return Buffer[Offset + offset];
			}
		}

		public void copy(int sourceOffset, byte[] targetArray, int targetOffset, int length)
		{
			Debug.Assert(sourceOffset + length <= Length);
			Array.Copy(Buffer, Offset + sourceOffset, targetArray, targetOffset, length);
		}

		public byte[] toArray()
		{
			var r = new byte[Length];
			Array.Copy(Buffer, Offset, r, 0, Length);
			return r;
		}
	}

	public static class BufferReferenceExtensions
	{
		public static BufferReference asBufferReference(this byte[] buffer)
		{
			return new BufferReference(buffer, 0, buffer.Length);
		}

		public static BufferReference asBufferReference(this byte[] buffer, int offset, int length)
		{
			Debug.Assert(buffer.Length >= offset + length);
			return new BufferReference(buffer, offset, length);
		}

		// Note: this is not symmetric, position is set to Offset!

		public static MemoryStream asPositionedStream(this BufferReference r)
		{
			return new MemoryStream(r.Buffer, r.Offset, r.Length);
		}
	}
}
