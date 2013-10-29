#if DEBUG

using System.Text;
using NUnit.Framework;
using Toolbox.IO;

namespace Toolbox.Mime
{
	[TestFixture]
	public sealed class QuotedPrintableTests
	{
		[Test]
		public void test()
		{
			for (int i = 0; i != Tests.Length; i += 2)
			{
				var from = Tests[i];
				var to = Tests[i+1];

				var toRes = encode(from);
				Assert.AreEqual(to, toRes, (i/2).ToString());
				var fromRes = decode(toRes);
				Assert.AreEqual(from, fromRes, (i/2).ToString());
			}
		}


		static string encode(string str)
		{
			var inBytes = Encoding.UTF8.GetBytes(str);

			var encoder = new QuotedPrintableEncoder(inBytes.toInStream());

			var encoded = encoder.toArray();
			return Encoding.UTF8.GetString(encoded);
		}

		static string decode(string str)
		{
			var inBytes = Encoding.UTF8.GetBytes(str);

			var encoder = new QuotedPrintableDecoder(inBytes.toInStream());

			var encoded = encoder.toArray();
			return Encoding.UTF8.GetString(encoded);
		}

		static readonly string[] Tests = new string[]
		{
			"A\r\nB", "A\r\nB",
			// we treat isolated LF / CR as binary
			"A\nB", "A=0AB",
			"A\rB", "A=0DB"
		};
	}
}

#endif
