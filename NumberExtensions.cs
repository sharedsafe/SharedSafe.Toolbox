using System;
using System.Diagnostics;

namespace Toolbox
{
	public static class NumberExtensions
	{
		public static uint unsigned(this int i)
		{
			Debug.Assert(i >= 0);
			return (uint) i;
		}

		public static ulong unsigned(this long s)
		{
			Debug.Assert(s >= 0);
			return (ulong) s;
		}

		public static int signed(this uint i)
		{
			Debug.Assert(i <= int.MaxValue);
			return (int) i;
		}

		public static long signed(this ulong us)
		{
			Debug.Assert(us <= long.MaxValue);
			return (long) us;
		}

		public static int rounded(this double d)
		{
			return (int)Math.Round(d);
		}
	}
}
