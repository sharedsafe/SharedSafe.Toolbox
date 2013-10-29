using System.Collections.Generic;
using System.Linq;
using RootSE.Provider;

namespace RootSE.ORM
{
	public static class Schema
	{
		public static bool isCompatibleTableExisting<TypeT>(this IStorageProvider provider)
		{
			var tableName = ORM<TypeT>.TableName;

			if (!provider.hasTable(tableName))
				return false;

			var ourColumns = ORM<TypeT>.Columns;
			var existingColumns = provider.getColumns(tableName);

			if (!columnsEqual(ourColumns, existingColumns))
				return false;

			var ourIndices = ORM<TypeT>.Indices;
			var existingIndices = provider.getIndices(tableName);
			
			return indicesEqual(ourIndices, existingIndices);
		}

		static bool columnsEqual(IEnumerable<Column> left, IEnumerable<Column> right)
		{
			return new HashSet<Column>(left).SetEquals(right);
		}

		static bool indicesEqual(IEnumerable<Index> left, IEnumerable<Index> right)
		{
			return new HashSet<Index>(left).SetEquals(right);
		}
	}
}
