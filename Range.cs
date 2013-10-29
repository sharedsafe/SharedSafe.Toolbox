namespace Toolbox
{
	/**
		We define End not to be included in the range.
	**/

	public struct Range<ValueT>
	{
		public readonly ValueT Begin;
		public readonly ValueT End;

		public ValueT Top { get { return Begin; } }
		public ValueT Bottom { get { return End; } }

		public Range(ValueT begin, ValueT end)
		{
			Begin = begin;
			End = end;
		}

		public override bool Equals(object obj)
		{
			return obj is Range<ValueT> && (Range<ValueT>)obj == this;
		}

		public override int GetHashCode()
		{
			int r = 0;

			if (Begin != null)
				r ^= Begin.GetHashCode();

			if (End != null)
				r ^= Begin.GetHashCode();

			return r;
		}

		public static bool operator ==(Range<ValueT> l, Range<ValueT> r)
		{
			return Equals(l.Begin, r.Begin) && Equals(l.End, r.End);
		}

		public static bool operator !=(Range<ValueT> l, Range<ValueT> r)
		{
			return !(l == r);
		}
	}

	public static class Range
	{
		public static Range<ValueT> make<ValueT>(ValueT begin, ValueT end)
		{
			return new Range<ValueT>(begin, end);
		}
	}
}
