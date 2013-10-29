using System;
using System.Collections.Generic;
using RootSE.Provider.RTree;
using Toolbox;
using Toolbox.Constraints;

namespace RootSE.Provider
{
	[Pure]
	public interface IStorageProvider : IDisposable
	{
		#region Schema

		bool hasTable(string tableName);
		// I think that columnvalues should not define the Id
		void createTable(string tableName, params Column[] columns);
		void deleteTable(string tableName);

		Column[] getColumns(string tableName);
		void addColumn(string columnName, Column column);

		void createIndex(string tableName, Index index);
		Index[] getIndices(string tableName);

		#endregion

		IProviderTransaction beginTransaction();
		bool IsTransacting { get; }

		ulong insert(string tableName, params ColumnValue[] values);
		uint update(string tableName, ColumnValue[] values, string whereClause);
		uint delete(string tableName, params ColumnValue[] values);
		uint delete(string tableName, string whereClause);

		#region Query

		// ORM:
		IEnumerable<ResultT> query<ResultT>(
			string tableName, 
			string whereClause = "",
			string orderByClause = "",
			string limitClause = "");

		// involve additional tables (tableNames.First() is the result table and whereClause needs to use qualified column names in the expressions).
		IEnumerable<ResultT> query<ResultT>(IEnumerable<string> tableNames, string whereClause);

		// this should be called queryColumn()
		IEnumerable<ResultT> queryColumn<ResultT>(string tableName, string columnName, string whereClause = "");
		IEnumerable<ResultT> queryColumnByKey<ResultT>(string tableName, string keyColumn, object keyValue, string resultColumn);

		IEnumerable<Pair<RT1, RT2>> queryTwoColumnsByKey<RT1, RT2>(string tableName,
			string keyColumn,
			object keyValue,
			string resultColumn1,
			string resultColumn2);

		IEnumerable<Pair<KeyT, ResultT>> queryRelation<KeyT, ResultT>(RelationQuery query, object keyValue);

		ulong queryCount(string tableName);

		#endregion

		void updateByKey(
			string tableName, 
			string keyColumn, 
			object keyValue, 
			string valueColumn, 
			object value);

		IRTreeProvider RTree { get; }
	}
}
