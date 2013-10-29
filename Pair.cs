using System;
using System.Collections.Generic;

namespace Toolbox
{
	public struct Pair<FirstT, SecondT> : IComparable<Pair<FirstT, SecondT>>
	{
		public readonly FirstT First;
		public readonly SecondT Second;

		private static readonly IComparer<FirstT> FirstComparer = Comparer<FirstT>.Default;
		private static readonly IComparer<SecondT> SecondComparer = Comparer<SecondT>.Default;

		public Pair(FirstT first, SecondT second)
		{
			First = first;
			Second = second;
		}

		public int CompareTo(Pair<FirstT, SecondT> other)
		{
			int firstCompare = FirstComparer.Compare(First, other.First);
			return firstCompare != 0 ? firstCompare : SecondComparer.Compare(Second, other.Second);
		}

		public override bool Equals(object obj)
		{
			return obj is Pair<FirstT, SecondT> && (Pair<FirstT, SecondT>)obj == this;
		}

		public override int GetHashCode()
		{
			int r = 0;

			if (First != null)
				r ^= First.GetHashCode();

			if (Second != null)
				r ^= Second.GetHashCode();

			return r;
		}

		public static bool operator ==(Pair<FirstT, SecondT> l, Pair<FirstT, SecondT> r)
		{
			return Equals(l.First, r.First) && Equals(l.Second, r.Second);
		}

		public static bool operator !=(Pair<FirstT, SecondT> l, Pair<FirstT, SecondT> r)
		{
			return !(l == r);
		}
	}

	public static class Pair
	{
		public static Pair<FirstT, SecondT> make<FirstT, SecondT>(FirstT first, SecondT second)
		{
			return new Pair<FirstT, SecondT>(first, second);
		}
	}
}
