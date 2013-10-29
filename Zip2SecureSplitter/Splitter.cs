using System;
using System.IO;
using Toolbox;

namespace Zip2SecureSplitter
{
	static class Splitter
	{
		public static void split(string filename)
		{
			var contents = File.ReadAllBytes(filename);
			var both = split(contents);
			
			
			File.WriteAllBytes(makeHeadFilename(filename), both.First);
			File.WriteAllBytes(makeTailFilename(filename), both.Second);
		}

		public static void join(string filename)
		{
			var head = File.ReadAllBytes(makeHeadFilename(filename));
			var tail = File.ReadAllBytes(makeTailFilename(filename));
			var res = join(head, tail);
			File.WriteAllBytes(filename, res);
		}

		#region Split

		static Two<byte[]> split(byte[] input)
		{
			int? offset = tryFindMarker(input);
			if (offset == null)
				throw new Exception("Marker not found.");


			var first = new byte[offset.Value];
			var second = new byte[input.Length - Marker.Length - offset.Value];
			Array.Copy(input, first, first.Length);
			Array.Copy(input, offset.Value + Marker.Length, second, 0, second.Length);

			return Two.make(first, second);
		}

		static int? tryFindMarker(byte[] sequence)
		{
			for (var i = 0; i != sequence.Length; ++i)
				if (isAtMarker(sequence, i))
					return i;
			return null;
		}


		static bool isAtMarker(byte[] sequence, int offset)
		{
			if (offset + Marker.Length > sequence.Length)
				return false;

			for (int i = 0; i != Marker.Length; ++i)
			{
				if (sequence[offset + i] != Marker[i])
					return false;
			}

			return true;
		}

		#endregion

		#region Join

		static byte[] join(byte[] head, byte[] tail)
		{
			var r = new byte[head.Length + Marker.Length + tail.Length];

			Array.Copy(head, r, head.Length);
			Array.Copy(Marker, 0, r, head.Length, Marker.Length);
			Array.Copy(tail, 0, r, head.Length + Marker.Length, tail.Length);

			return r;
		}

		#endregion

		static string makeHeadFilename(string baseFilename)
		{
			return baseFilename + "_head";
		}

		static string makeTailFilename(string baseFilename)
		{
			return baseFilename + "_tail";
		}

		static readonly byte[] Marker = new byte[]
		{

			0x07,
			0xD2,
			0x6C,
			0xBF,
			0x21,
			0x59,
			0xAB,
			0xAA
		};
	}

}
