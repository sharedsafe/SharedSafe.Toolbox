using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Toolbox.Meta;

namespace RootSE.Provider
{
	public static class StorageProviderORMExtensions
	{
		#region Implicit Table Name for Type

		public static bool hasTableForType<TypeT>(this IStorageProvider storageProvidera)
		{
			return storageProvidera.hasTable(tableNameForType<TypeT>());
		}
		
		public static void createTableAndIndicesForType<TypeT>(this IStorageProvider storageProvider)
		{
			createTableAndIndicesForType<TypeT>(storageProvider, tableNameForType<TypeT>());
		}

		public static ulong insert<TypeT>(this IStorageProvider storageProvider, TypeT instance)
		{
			return storageProvider.insert(tableNameForType<TypeT>(), instance);
		}

		public static uint update<TypeT>(this IStorageProvider storageProvider, TypeT instance)
		{
			return storageProvider.update(tableNameForType<TypeT>(), instance);
		}

		public static void delete<TypeT>(this IStorageProvider storageProvider, params ColumnValue[] whereValues)
		{
			storageProvider.delete(tableNameForType<TypeT>(), whereValues);
		}

		public static void delete<TypeT>(this IStorageProvider storageProvider, Criteria criteria)
		{
			storageProvider.delete(tableNameForType<TypeT>(), criteria.SQL);
		}

		public static IEnumerable<TypeT> queryORM<TypeT>(
			this IStorageProvider storageProvider, 
			Criteria whereClause, 
			OrderBy orderBy_ = null, 
			Limit limit_ = null)
		{
			return storageProvider.query<TypeT>(tableNameForType<TypeT>(), 
				whereClause.SQL,
				orderBy_ != null ? orderBy_.SQL : string.Empty,
				limit_ != null ? limit_.SQL : string.Empty);
		}

		public static IEnumerable<TypeT> queryORM<TypeT>(this IStorageProvider storageProvider, string whereClause = "")
		{
			return storageProvider.query<TypeT>(tableNameForType<TypeT>(), whereClause);
		}

		public static TypeT tryGetByPrimaryKey<TypeT>(this IStorageProvider storageProvider, object primaryKeyValue)
		{
			var primaryColumnName = ORM<TypeT>.PrimaryKeyColumnName_;
			Debug.Assert(primaryColumnName != null);
			
			return storageProvider.query<TypeT>(tableNameForType<TypeT>(),
				Term.column(primaryColumnName).equals(primaryKeyValue))
				.SingleOrDefault();
		}

		public static ulong queryCount<TypeT>(this IStorageProvider storageProvider)
		{
			return storageProvider.queryCount(tableNameForType<TypeT>());
		}
		
		static string tableNameForType<TypeT>()
		{
			return ORM<TypeT>.TableName;
		}

		#endregion

		public static void createTableAndIndicesForType<TypeT>(this IStorageProvider storageProvider, string tableName)
		{
			storageProvider.createTable(tableName, ORM<TypeT>.Columns);

			foreach (var index in ORM<TypeT>.Indices)
			{
				storageProvider.createIndex(tableName, index);
			}
		}

		public static ulong insert<TypeT>(this IStorageProvider provider, string tableName, TypeT instance)
		{
			var columnNames = ORM<TypeT>.ColumnNames;
			var fields = ORM<TypeT>.Fields;
			var flags = ORM<TypeT>.Flags;

			object inst = instance;

			var values = new ColumnValue[columnNames.Length];

			for (int i = 0; i != columnNames.Length; ++i)
			{
				bool rowId = (flags[i] & FieldFlags.RowId) != 0;
				var value = rowId ? null : fields[i].GetValue(inst);
				values[i] = new ColumnValue(columnNames[i], value);
			}

			var newRowId = provider.insert(tableName, values);

			var rowIdSetter_ = ORM<TypeT>.RowIdSetter_;
			if (rowIdSetter_ != null)
				rowIdSetter_(instance, newRowId);

			return newRowId;
		}

		public static uint update<TypeT>(this IStorageProvider provider, string tableName, TypeT instance)
		{
			string whereClause_ = tryBuildWhereClauseToMatchInstance(instance);
			Debug.Assert(whereClause_ != null);

			var primaryKeyIndex = ORM<TypeT>.PrimaryKeyIndex.Value;
			Debug.Assert(ORM<TypeT>.PrimaryKeyIndex != null);
			var columnNames = ORM<TypeT>.ColumnNames;
			var fields = ORM<TypeT>.Fields;
			var values = new ColumnValue[fields.Length - 1];

			int ti = 0;
			for (int i = 0; i != fields.Length; ++i)
			{
				if (i == primaryKeyIndex)
					continue;

				var columnName = columnNames[i];
				var value = fields[i].GetValue(instance);
				values[ti++] = new ColumnValue(columnName, value);
			}

			return provider.update(tableName, values, whereClause_);
		}

		public static uint delete<TypeT>(this IStorageProvider provider, string tableName, TypeT instance)
		{
			var whereClause_ = tryBuildWhereClauseToMatchInstance(instance);
			Debug.Assert(whereClause_ != null);
			return provider.delete(tableName, whereClause_);
		}

		static string tryBuildWhereClauseToMatchInstance<TypeT>(TypeT instance)
		{
			var fields = ORM<TypeT>.Fields;
			var columnNames = ORM<TypeT>.ColumnNames;
			var primaryKeyIndex = ORM<TypeT>.PrimaryKeyIndex.Value;
			var value = fields[primaryKeyIndex].GetValue(instance);
			var columnName = columnNames[primaryKeyIndex];

			return SQLSyntax.equalValueExpression(columnName, value);
		}

		public static IEnumerable<ResultT> queryWhereEqual<ResultT, CompareT>(this IStorageProvider provider, string tableName, Expression<Func<ResultT, CompareT>> memberAccessor, CompareT value)
		{
			var columnName = memberAccessor.nameOfMember();
			var whereClause = SQLSyntax.equalValueExpression(columnName, value);
			return provider.query<ResultT>(tableName, whereClause);
		}
	}
}
