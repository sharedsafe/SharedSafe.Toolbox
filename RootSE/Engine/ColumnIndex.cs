using System;
using Toolbox;

namespace RootSE.Engine
{
	public enum IndexKind
	{
		Unique,
		Multiple
	}

	sealed class ColumnIndex
	{
		public readonly DocumentTable Table;
		public readonly IndexKind Kind;
		public readonly string ColumnName;
		public readonly Func<object, object> GetKey;

		public ColumnIndex(DocumentTable table, IndexKind kind, string columnName, Func<object, object> getKey)
		{
			Table = table;
			Kind = kind;
			ColumnName = columnName;
			GetKey = getKey;
		}

		public bool IsSane { get; set; }

		public override string ToString()
		{
			return "ColumnIndex {0}.{1} (sane: {2})".format(Table.Name, ColumnName, IsSane);
		}
	}
}
