namespace Toolbox
{
	/**
		Extension methods for bool? operations.
	**/

	public static class TriState
	{
		public static bool? and(this bool l, bool? r)
		{
			return ((bool?)l).and(r);
		}
		public static bool? or(this bool l, bool? r)
		{
			return ((bool?)l).or(r);
		}
		public static bool? and(this bool? l, bool? r)
		{
			if (l == null || r == null)
				return null;
			return l.Value && r.Value;
		}
		public static bool? or(this bool? l, bool? r)
		{
			if (!l.HasValue)
				return r;
			if (!r.HasValue)
				return l;
			return l.Value || r.Value;
		}

		public static bool? not(this bool? b)
		{
			// a not of "undecided" is "undecided"
			if (b == null)
				return null;
			return !b.Value;
		}
	}
}
