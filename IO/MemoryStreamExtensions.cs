using System;
using System.IO;

namespace Toolbox.IO
{
	public static class MemoryStreamExtensions
	{
		public static void recycle(this MemoryStream stream)
		{
			stream.SetLength(0);
		}

		public static BufferReference asBufferReference(this MemoryStream stream)
		{
			if (stream.Length > int.MaxValue)
				throw new NotImplementedException("Stream too long");
			return new BufferReference(stream.GetBuffer(), 0, (int)stream.Length);
		}
	}
}
