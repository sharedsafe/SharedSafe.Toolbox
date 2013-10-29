using System.Collections.Generic;
using System.Linq;

namespace RootSE.Provider
{
	public struct Index
	{
		public Index(IEnumerable<string> columnNames, bool unique)
		{
			ColumnNames = columnNames.ToArray();
			Unique = unique;
		}

		public readonly string[] ColumnNames;
		public readonly bool Unique;

		public bool Equals(Index other)
		{
			return other.ColumnNames.SequenceEqual(ColumnNames) && other.Unique.Equals(Unique);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Index))
				return false;
			return Equals((Index) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (ColumnNames.Length.GetHashCode()*397) ^ Unique.GetHashCode();
			}
		}
	}
}
