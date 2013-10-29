using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using RootSE.Provider.RTree;
using Toolbox;
using Toolbox.Constraints;

namespace RootSE.Provider.SQLite
{
	[Pure]
	sealed class SQLiteProvider : IStorageProvider
	{
		readonly IValueSerializer _serializer;
		readonly SQLiteImplementation _db;
		readonly RTreeProvider _rtree;

		public SQLiteProvider(string database, bool createNewOfNotExisting, StorageProviderOptions options)
		{
			this.D("creating for " +database);

			if (!createNewOfNotExisting && !File.Exists(database))
				throw new Exception("Database file does not exist.");

			_serializer = options.Serializer_ ?? new NewtonsoftJsonValueSerializer();

			_db = new SQLiteImplementation(database, _serializer);

			// switch off synchronous mode (let OS take care about writes)
			// when we put all inserts into a transaction, this is fast enough!
			// _db.ExecuteNonQuery("PRAGMA synchronous=OFF;");
			
			// but leave journals open and overwrite them (don't truncate)
			// (the deleting of a journal costs a lot of performance and adds no security)
			_db.ExecuteNonQuery("PRAGMA journal_mode=PERSIST");
			if (options.WriteAsynchronous)
				_db.ExecuteNonQuery("PRAGMA synchronous=0");

			_rtree = new RTreeProvider(_db, this);

		}

		public void Dispose()
		{
			_db.Dispose();

			this.D("disposed");
		}

		public IRTreeProvider RTree
		{
			get { return _rtree; }
		}

		#region Meta

		public bool hasTable(string tableName)
		{
			var dt = _db.ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table' AND name={0};"
				.format(ValueEncoder.escapeString(tableName)));
			return dt.Rows.Count != 0;
		}

		public void createTable(string tableName, params Column[] columns)
		{
			var sb = new StringBuilder();

			sb.Append("CREATE TABLE ");
			sb.Append(Escape.table(tableName));
			sb.Append("(");

			var columDefs = from c in columns select Escape.column(c.Name) + " " + c.TypeAndConstraint;
			var allColumns = columDefs.Aggregate((a, b) => a + "," + b);

			sb.Append(allColumns);

			sb.Append(");");

			_db.ExecuteNonQuery(sb.ToString());
		}

		public void deleteTable(string tableName)
		{
			var str = "DROP TABLE " + Escape.table(tableName);
			_db.ExecuteNonQuery(str);
		}

		// http://stackoverflow.com/questions/928865/find-sqlite-column-names-in-empty-table

		public Column[] getColumns(string tableName)
		{
			using (var r = _db.ExecuteQuery("PRAGMA table_info({0});".format(Escape.table(tableName))))
			{
				// todo: read the column names from metadata
				const int NameColumn = 1;
				const int TypeColumn = 2;
				const int NotNullColumn = 3;
				const int PrimaryKeyColumn = 5;

				var columns = new Column[r.Rows.Count];

				int i = 0;
				foreach (DataRow row in r.Rows)
				{
					var name = (string) row[NameColumn];
					var type = (string) row[TypeColumn];
					var notNull = int.Parse((string) row[NotNullColumn]);
					var primaryKey = int.Parse((string)row[PrimaryKeyColumn]);

					if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
						throw new Exception("Name or type are not set in the column description.");

					columns[i++] = new Column(name, type, notNull != 0, primaryKey != 0);
				}

				return columns;
			}
		}

		public Index[] getIndices(string tableName)
		{
			var indexNames = getIndexNamesForTable(tableName);

			var indices = new Index[indexNames.Count()];
			var i = 0;

			foreach (var index in indexNames)
			{
				using (var r = _db.ExecuteQuery("PRAGMA index_info({0});".format(Escape.index(index.First))))
				{
					var columns = new string[r.Rows.Count];
					var column = 0;

					foreach (DataRow row in r.Rows)
					{
						const int ColumnNameIndex = 2;

						var columnName = (string) row[ColumnNameIndex];

						columns[column++] = columnName;
					}

					indices[i++] = new Index(columns, index.Second);
				}
			}

			return indices;
		}

		IEnumerable<Pair<string, bool>> getIndexNamesForTable(string tableName)
		{
			Pair<string, bool>[] indices;
			using (var r = _db.ExecuteQuery("PRAGMA index_list({0});".format(Escape.table(tableName))))
			{
				indices = new Pair<string, bool>[r.Rows.Count];

				int i = 0;
				foreach (DataRow row in r.Rows)
				{
					const int IndexNameColumnIndex = 1;
					const int IndexUniqueColumnIndex = 2;

					var name = (string) row[IndexNameColumnIndex];
					var unique = int.Parse((string) row[IndexUniqueColumnIndex]);
					indices[i++] = Pair.make(name, unique != 0);
				}
			}
			return indices;
		}

		public void addColumn(string tableName, Column column)
		{
			_db.ExecuteNonQuery("ALTER TABLE {0} ADD COLUMN {1} {2};".format(
				Escape.table(tableName), 
				Escape.column(column.Name), 
				column.TypeAndConstraint));
		}

		public void createIndex(string tableName, Index index)
		{
			var prefix = index.Unique ? "CREATE UNIQUE" : "CREATE";
			var indexName = tableName + "_" + string.Join("_", index.ColumnNames) + (index.Unique ? "_unique" : "");

			_db.ExecuteNonQuery("{0} INDEX {1} ON {2}({3});"
				.format(prefix, 
				Escape.index(indexName), 
				Escape.table(tableName), 
				string.Join(",", Escape.columns(index.ColumnNames))));
		}

		#endregion

		[ThreadStatic] 
		static TransactionCoordinator _transactionCoordinator;

		public IProviderTransaction beginTransaction()
		{
			if (_transactionCoordinator == null)
			{
				_db.ExecuteNonQuery("BEGIN TRANSACTION;");
				_transactionCoordinator = new TransactionCoordinator();
			}

			_transactionCoordinator.beginTransaction();
			return new Transaction(this);
		}

		internal void endTransaction(bool commit)
		{
			if (_transactionCoordinator == null)
				throw new Exception("No transaction active");

			_transactionCoordinator.endTransaction(commit);
			if (!_transactionCoordinator.ShouldEndTransaction)
				return;

			_db.ExecuteNonQuery(_transactionCoordinator.ShouldCommitTransaction
				? "COMMIT TRANSACTION;" 
				: "ROLLBACK TRANSACTION;");

			_transactionCoordinator = null;
		}

		public bool IsTransacting 
		{ get { return _transactionCoordinator != null; } }

		public ulong insert(string tableName, params ColumnValue[] values)
		{
			var sb = new StringBuilder();

			sb.Append("INSERT INTO ");
			sb.Append(Escape.table(tableName));
			sb.Append("(");
			var allColumns = (from v in values select Escape.column(v.Column)).Aggregate((a, b) => a + "," + b);
			sb.Append(allColumns);

			sb.Append(")VALUES(");

			var allValues = (from v in values select ValueEncoder.encode(v.Value, _serializer)).Aggregate((a, b) => a + "," + b);
			sb.Append(allValues);
			sb.Append(");");

			var sql = sb.ToString();
			_db.ExecuteNonQuery(sql);
			return _db.LastInsertId;
		}

		public uint update(string tableName, ColumnValue[] values, string whereClause)
		{
			var sb = new StringBuilder();

			sb.Append("UPDATE ");
			sb.Append(Escape.table(tableName));
			sb.Append(" SET ");

			var allValues = (from v in values select Escape.column(v.Column) + "=" + ValueEncoder.encode(v.Value, _serializer)).Aggregate((a, b) => a + "," + b);

			sb.Append(allValues);
			sb.Append(" WHERE ");
			sb.Append(whereClause);

			_db.ExecuteNonQuery(sb.ToString());
			return _db.Changes;
		}

		public uint delete(string tableName, params ColumnValue[] values)
		{
			var allWheres = values
				.Select(v => SQLSyntax.equalValueExpression(v.Column, v.Value))
				.Aggregate((a, b) => a + " AND " + b);

			return delete(tableName, allWheres);
		}

		public uint delete(string tableName, string whereClause)
		{
			var sb = new StringBuilder();
			sb
				.Append("DELETE FROM ")
				.Append(Escape.table(tableName));

			if (whereClause != string.Empty)
			{
				sb.Append(" WHERE ")
				.Append(whereClause);
			}

			_db.ExecuteNonQuery(sb.ToString());
			return _db.Changes;
		}

		public IEnumerable<TypeT> query<TypeT>(string tableName, string whereClause, string orderByClause, string limitClause)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			sb.Append(ORMHelper<TypeT>.ColumnQueryString);
			sb.Append(" FROM ");
			sb.Append(Escape.table(tableName));
			if (whereClause != string.Empty)
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}

			if (orderByClause != string.Empty)
			{
				sb.Append(" ORDER BY ");
				sb.Append(orderByClause);
			}

			if (limitClause != string.Empty)
			{
				sb.Append(" LIMIT ");
				sb.Append(limitClause);
			}

			sb.Append(";");

			return _db.queryColumns<TypeT>(sb.ToString());
		}

		public IEnumerable<ResultT> query<ResultT>(IEnumerable<string> tableNames, string whereClause)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			sb.Append(ORMHelper<ResultT>.getQualifiedColumnQueryString(tableNames.First()));
			sb.Append(" FROM ");
			sb.Append((from t in tableNames select Escape.table(t)).Aggregate((lt, rt) => lt + "," + rt));
			if (whereClause != string.Empty)
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}
			sb.Append(";");

			return _db.queryColumns<ResultT>(sb.ToString());
		}

		static class ORMHelper<TypeT>
		{
			static readonly string[] EscapedColumnNames =
				(from c in ORM<TypeT>.ColumnNames select Escape.column(c)).ToArray();

			public static readonly string ColumnQueryString = string.Join(",", EscapedColumnNames);

			public static string getQualifiedColumnQueryString(string tableName)
			{
				return getQualifiedEscapedColumnNames(tableName).Aggregate((left, right) => left + "," + right);
			}

			static IEnumerable<string> getQualifiedEscapedColumnNames(string tableName)
			{
				return from c in ORM<TypeT>.ColumnNames select Escape.qualifiedColumn(tableName, c);
			}
		}

		// tbd: check all projects so that we can sure that whereClause_ is always Empty and never null 
		// when Criteria.All is meant!

		public IEnumerable<TypeT> queryColumn<TypeT>(string tableName, string columnName, string whereClause_)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			sb.Append(Escape.column(columnName));
			sb.Append(" FROM ");
			sb.Append(Escape.table(tableName));

			if (!string.IsNullOrEmpty(whereClause_))
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause_);
			}

			sb.Append(';');

			return _db.querySingleColumn<TypeT>(sb.ToString());
		}

		public IEnumerable<TypeT> queryColumnByKey<TypeT>(string tableName, string keyColumn, object keyValue, string resultColumn)
		{
			var sql = "SELECT {0} FROM {1} WHERE {2};".format(
				Escape.column(resultColumn),
				Escape.table(tableName),
				SQLSyntax.equalValueExpression(keyColumn, keyValue));

			return _db.querySingleColumn<TypeT>(sql);
		}

		public IEnumerable<Pair<RT1,RT2>> queryTwoColumnsByKey<RT1, RT2>(string tableName, string keyColumn, object keyValue, string resultColumn1, string resultColumn2)
		{
			var sql = "SELECT {0},{1} FROM {2} WHERE {3};".format(
				Escape.column(resultColumn1),
				Escape.column(resultColumn2),
				Escape.table(tableName),
				SQLSyntax.equalValueExpression(keyColumn, keyValue));

			return _db.queryTwoColumns<RT1, RT2>(sql);
		}

		public IEnumerable<Pair<KeyT, ResultT>> queryRelation<KeyT, ResultT>(RelationQuery query, object keyValue)
		{
			var qualifiedDocumentRelationColumn = query.DocumentTable + "." + query.DocumentRelationColumn;
			var qualifiedRelationDocumentColumn = query.RelationTable + "." + query.RelationDocumentColumn;
			var qualifiedRelationKeyColumn = query.RelationTable + "." + query.RelationKeyColumn;

			var sql = "SELECT {0},{1} FROM {2} INNER JOIN {3} ON {4} WHERE {5};".format(
				Escape.column(qualifiedDocumentRelationColumn),
				Escape.table(query.DocumentTable) + "." + Escape.column(query.DocumentResultColumn),

				Escape.table(query.DocumentTable),
				Escape.table(query.RelationTable),

				Escape.column(qualifiedDocumentRelationColumn) + 
				"=" +
				Escape.column(qualifiedRelationDocumentColumn),

				SQLSyntax.equalValueExpression(qualifiedRelationKeyColumn, keyValue)
				);

			return _db.queryTwoColumns<KeyT, ResultT>(sql);

		}

		public ulong queryCount(string tableName)
		{
			var sql = "SELECT COUNT(1) FROM {0};".format(Escape.table(tableName));
			return _db.querySingleColumn<ulong>(sql).Single();
		}

		public void updateByKey(string tableName, string keyColumn, object keyValue, string valueColumn, object value)
		{
			var sql = "UPDATE {0} SET {1}={2} WHERE {3};".format(
				Escape.table(tableName),
				Escape.column(valueColumn),
				ValueEncoder.encode(value, _serializer),
				SQLSyntax.equalValueExpression(keyColumn, keyValue)
				);

			_db.ExecuteNonQuery(sql);
		}

		sealed class Transaction : IProviderTransaction
		{
			readonly SQLiteProvider _provider;
			bool _commited;

			public Transaction(SQLiteProvider provider)
			{
				_provider = provider;
			}

			public void commit()
			{
				if (_commited)
					throw new Exception("Transaction already commited");

				_provider.endTransaction(true);
				_commited = true;
			}

			public void Dispose()
			{
				if (_commited)
					return;

				_provider.endTransaction(false);
			}
		}
	}
}
