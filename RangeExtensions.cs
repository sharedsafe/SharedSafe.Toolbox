
using System.Collections.Generic;

namespace Toolbox
{
	public static class RangeExtensions
	{
		public static Range<int> to(this int from, int to)
		{
			return Range.make(from, to+1);
		}

		public static Range<uint> to(this uint from, uint to)
		{
			return Range.make(from, to+1);
		}

		public static IEnumerable<int> values(this Range<int> r)
		{
			for (int i = r.Begin; i != r.End; ++i)
				yield return i;
		}

		public static IEnumerable<uint> values(this Range<uint> r)
		{
			for (uint i = r.Begin; i != r.End; ++i)
				yield return i;
		}
	}
}
