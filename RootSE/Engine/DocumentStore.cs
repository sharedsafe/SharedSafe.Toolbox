using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using RootSE.Provider;
using Toolbox.Meta;

namespace RootSE.Engine
{
	sealed class DocumentStore
	{
		readonly IStorageProvider _provider;
		readonly DocumentTables _tables;
		readonly DocumentSerializer _serializer;

		public DocumentStore(IStorageProvider provider, DocumentTables tables, DocumentSerializer serializer)
		{
			_provider = provider;
			_tables = tables;
			_serializer = serializer;
		}

		public void store(object obj)
		{
			var table = _tables.getOrCreateForType(obj.GetType());
			var value = _serializer.serialize(obj);
			_tables.insert(table, value);
		}

		public IEnumerable<object> queryAll(Type t)
		{
			// this may create an empty table (may be we'll find a better way here)
			var table = _tables.getOrCreateForType(t);

			return from d in _tables.queryAll(table) select _serializer.deserialize(d, t);
		}

		public IEnumerable<DocumentT> queryByKey<DocumentT, KeyT>(Expression<Func<DocumentT, KeyT>> keyMember, string keyValue)
		{
			var documentType = typeof(DocumentT);
			var keyType = typeof(KeyT);

			var table = _tables.getOrCreateForType(documentType);

			var indexName = keyMember.nameOfMember();
			var columnIndex = table.tryGetColumnIndex(indexName);
			if (columnIndex == null)
			{
				Func<DocumentT, KeyT> typedMemberAccessor = keyMember.Compile();
				Func<object, object> memberAccessor = t => typedMemberAccessor((DocumentT)t);
				columnIndex = _tables.getOrCreateColumnIndex(table, false, indexName, keyType, memberAccessor);
			}

			if (!columnIndex.IsSane)
				_tables.sanitizeColumnIndex(columnIndex, documentType, _serializer);

			Debug.Assert(columnIndex.IsSane);

			return
				from d in _tables.queryDocumentByKey(columnIndex, keyValue)
				select (DocumentT)_serializer.deserialize(d.Second, documentType);
		}
	}
}
