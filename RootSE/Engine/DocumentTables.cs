using System;
using System.Collections.Generic;
using RootSE.Provider;
using Toolbox;

namespace RootSE.Engine
{
	/**
		Lazily retrieves DB related schema data.
	**/

	sealed class DocumentTables
	{
		readonly IStorageProvider _provider;
		readonly Dictionary<string, DocumentTable> _tables = new Dictionary<string, DocumentTable>();

		public DocumentTables(IStorageProvider provider)
		{
			_provider = provider;
		}

		public DocumentTable getOrCreateForType(Type t)
		{
			DocumentTable vt;
			var tableName = t.Name;

			if (_tables.TryGetValue(tableName, out vt))
				return vt;
			var hasTable = _provider.hasTable(tableName);
			vt = !hasTable ? create(tableName) : new DocumentTable(tableName);
			_tables.Add(tableName, vt);
			return vt;
		}

		DocumentTable create(string tableName)
		{
			_provider.createTable(tableName, InitialValueTableColumns);
			return new DocumentTable(tableName);
		}

		#region Commands

		public void insert(DocumentTable table, string document)
		{
			_provider.insert(table.Name, new ColumnValue(ValueColumnName, document));
			table.notifyDocumentsChanged();
		}

		public IEnumerable<string> queryAll(DocumentTable table)
		{
			return _provider.queryColumn<string>(table.Name, ValueColumnName);
		}

		#endregion


		#region Indices

		public ColumnIndex getOrCreateColumnIndex(DocumentTable table, bool unique, string columnName, Type indexType, Func<object, object> accessor)
		{
			var columnIndex = table.tryGetColumnIndex(columnName);
			if (columnIndex != null)
				return columnIndex;

			if (!_provider.hasColumn(table.Name, columnName))
			{
				_provider.addColumn(table.Name, new Column(columnName, Datatypes.toSQL(indexType), false, false));
				_provider.createColumnIndex(table.Name, columnName, unique);
			}

			columnIndex = new ColumnIndex(table, unique ? IndexKind.Unique : IndexKind.Multiple, columnName, accessor);
			table.addColumnIndex(columnIndex);

			return columnIndex;
		}

		/**
			Note: this is not yet transaction safe for multiple clients.
		**/

		public IEnumerable<Pair<long, string>> queryDocumentByKey(ColumnIndex index, object keyValue)
		{
			if (!index.IsSane)
				throw new Exception("Failed to query dirty ColumnIndex: " + index);

			var table = index.Table;

			return _provider.queryTwoColumnsByKey<long, string>(
				table.Name, 
				index.ColumnName, 
				keyValue, 
				IdColumnName,
				ValueColumnName);
		}

		public void sanitizeColumnIndex(ColumnIndex columnIndex, Type documentType, DocumentSerializer serializer)
		{
			// todo: this should be wrapped in a nested transaction so that it is FAST :)

			var tableName = columnIndex.Table.Name;

			var allNulls = _provider.queryTwoColumnsByKey<long, string>(
				tableName,
				columnIndex.ColumnName,
				null,
				IdColumnName,
				ValueColumnName);

			var keyColumn = columnIndex.ColumnName;

			foreach (var doc in allNulls)
			{
				var deserializedDocument = serializer.deserialize(doc.Second, documentType);
				var key = columnIndex.GetKey(deserializedDocument);
				if (key == null)
					throw new Exception("Key {0} in {1} can not be null".format(columnIndex.ColumnName, deserializedDocument));

				_provider.updateByKey(tableName, IdColumnName, doc.First, keyColumn, key);
			}

			columnIndex.IsSane = true;
		}

		#endregion


		public const string IdColumnName = Conventions.ReservedPrefix + "id";
		public const string ValueColumnName = Conventions.ReservedPrefix + "value";

		static readonly Column[] InitialValueTableColumns = new Column[]
		{
			new Column(IdColumnName, "INTEGER", false, true),
			new Column(ValueColumnName, "TEXT", false, false)
		};

	
	}
}
