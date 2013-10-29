using System.IO;

namespace Toolbox.IO
{
	public interface IOutStream
	{
		void writeBytes(byte[] array, uint offset, uint length);
	}

	public static partial class OutStreamExtensions
	{
		#region Stream extensions

		public static IOutStream toOutStream(this Stream stream)
		{
			return new StreamOutStream(stream);
		}

		sealed class StreamOutStream : IOutStream
		{
			readonly Stream _stream;

			public StreamOutStream(Stream stream)
			{
				_stream = stream;
			}

			#region IInStream Members

			public void writeBytes(byte[] array, uint offset, uint length)
			{
				_stream.Write(array, (int)offset, (int)length);
			}

			#endregion
		}

		#endregion
	}
}
