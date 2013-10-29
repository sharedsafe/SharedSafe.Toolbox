using Toolbox.IO;

namespace Toolbox.Mime
{
	public static class Extensions
	{
		#region

		public static IInStream decode(this byte[] bytes, EncodingType encoding)
		{
			return bytes.toInStream().decode(encoding);
		}

		public static IInStream encode(this byte[] bytes, EncodingType encoding)
		{
			return bytes.toInStream().encode(encoding);
		}

		#endregion

		#region IInStream

		public static IInStream decode(this IInStream input, EncodingType encoding)
		{
			switch (encoding)
			{
				case EncodingType.Base64:
					return new Base64Decoder(input);

				case EncodingType.QuotedPrintable:
					return new QuotedPrintableDecoder(input);

				default:
					return input;
			}
		}

		public static IInStream encode(this IInStream input, EncodingType encoding)
		{
			switch (encoding)
			{
				case EncodingType.Base64:
					return new Base64Encoder(input);

				case EncodingType.QuotedPrintable:
					return new QuotedPrintableEncoder(input);

				default:
					return input;
			}
		}

		#endregion
	}
}
