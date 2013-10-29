using System.Collections.Generic;
using System.Linq;
using RootSE.Provider;

namespace RootSE.Engine
{
	sealed class RelationStorage
	{
		readonly IStorageProvider _provider;
		readonly DocumentTables _tables;
		readonly DocumentSerializer _serializer;
		readonly ISession _session;

		public RelationStorage(IStorageProvider provider, DocumentTables tables, DocumentSerializer serializer, ISession session)
		{
			_provider = provider;
			_tables = tables;
			_serializer = serializer;
			_session = session;
		}

		public void connect<FromT, ToT>(string tableName, FromT from, ToT to)
		{
			connect(tableName, _session.idOf(from), idOf(to));
		}

		public void disconnect<FromT, ToT>(string tableName, FromT from, ToT to)
		{
			disconnect(tableName, _session.idOf(from), idOf(to));
		}

		public IEnumerable<ToT> queryForward<FromT, ToT>(string tableName, FromT from)
		{
			var toType = typeof (ToT);
			var documentTable = _tables.getOrCreateForType(toType);
			var documentTableName = documentTable.Name;
			var keyValue = idOf(from);

			var relationQuery = new RelationQuery
			{
				DocumentTable = documentTableName,
				DocumentRelationColumn = DocumentTables.IdColumnName,
				DocumentResultColumn = DocumentTables.ValueColumnName,

				RelationTable = tableName,
				
				RelationKeyColumn = FromColumn,
				RelationDocumentColumn = ToColumn
			};

			var res = _provider.queryRelation<long, string>(relationQuery, keyValue);
			return (from r in res select _serializer.deserialize<ToT>(r.Second));

		}

		public IEnumerable<FromT> queryBackward<FromT, ToT>(string tableName, ToT to)
		{
			var fromType = typeof(FromT);
			var documentTable = _tables.getOrCreateForType(fromType);
			var documentTableName = documentTable.Name;
			var keyValue = idOf(to);

			var relationQuery = new RelationQuery
			{
				DocumentTable = documentTableName,
				DocumentRelationColumn = DocumentTables.IdColumnName,
				DocumentResultColumn = DocumentTables.ValueColumnName,

				RelationTable = tableName,

				RelationKeyColumn = ToColumn,
				RelationDocumentColumn = FromColumn
			};

			var res = _provider.queryRelation<long, string>(relationQuery, keyValue);
			return (from r in res select _serializer.deserialize<FromT>(r.Second));
		}

		long idOf(object o)
		{
			return _session.idOf(o);
		}

		#region Bare

		IEnumerable<long> queryForward(string tableName, long from)
		{
			return _provider.queryColumnByKey<long>(tableName, FromColumn, from, ToColumn);
		}

		IEnumerable<long> queryBackward(string tableName, long to)
		{
			return _provider.queryColumnByKey<long>(tableName, ToColumn, to, FromColumn);
		}

		void connect(string tableName, long from, long to)
		{
			_provider.insert(tableName, 
				new ColumnValue(FromColumn, from), 
				new ColumnValue(ToColumn, to));
		}

		void disconnect(string tableName, long from, long to)
		{
			_provider.delete(tableName,
				new ColumnValue(FromColumn, from),
				new ColumnValue(ToColumn, to)
				);
		}

		#endregion


		public IRelation<FromT, ToT> createRelation<FromT, ToT>(string name)
		{
			var r = new Relation<FromT, ToT>(this, name);
			// we decided to eagerly ensure that the table exists.
			ensureTableExists(r.TableName);
			return r;
		}

		void ensureTableExists(string tableName)
		{
			if (_provider.hasTable(tableName))
				return;
			_provider.createTable(tableName, RelationColumns);
			_provider.createColumnIndex(tableName, FromColumn, false);
			_provider.createColumnIndex(tableName, ToColumn, false);
		}

		const string FromColumn = "from";
		const string ToColumn = "to";

		static readonly Column[] RelationColumns = new Column[]
		{
			new Column("from", "INTEGER", false, false),
			new Column("to", "INTEGER", false, false)
		};
	}
}
