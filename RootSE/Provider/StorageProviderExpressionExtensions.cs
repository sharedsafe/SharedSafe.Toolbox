using System.Collections.Generic;

namespace RootSE.Provider
{
	public static class StorageProviderExpressionExtensions
	{
		public static uint update(this IStorageProvider provider, string tableName, ColumnValue[] values, Criteria whereClause)
		{
			return provider.update(tableName, values, whereClause.SQL);
		}

		public static IEnumerable<TypeT> query<TypeT>(this IStorageProvider provider, string tableName, Criteria whereClause)
		{
			return provider.query<TypeT>(tableName, whereClause.SQL);
		}

		public static IEnumerable<TypeT> queryColumn<TypeT>(this IStorageProvider provider, string tableName, string columnName, Criteria whereClause)
		{
			return provider.queryColumn<TypeT>(tableName, columnName, whereClause.SQL);
		}
	}
}
