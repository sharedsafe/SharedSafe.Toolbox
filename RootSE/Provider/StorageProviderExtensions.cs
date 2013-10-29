using System;
using System.Linq;

namespace RootSE.Provider
{
	static class StorageProviderExtensions
	{
		public static bool hasColumn(this IStorageProvider _, string tableName, string columnName)
		{
			var allColumns = _.getColumns(tableName);
			return allColumns.Any(c => c.Name == columnName);
		}

		public static void createColumnIndex(this IStorageProvider provider, string tableName, string column, bool unique)
		{
			provider.createIndex(tableName, new Index(new[] {column}, unique));
		}

		public static void transact(this IStorageProvider provider, Action action)
		{
			using (var t = provider.beginTransaction())
			{
				action();
				t.commit();
			}
		}
	}
}