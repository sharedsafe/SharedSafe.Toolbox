using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using RootSE.Provider;
using Toolbox;

namespace RootSE.Engine
{
	sealed class ProviderStorage : IStorage
	{
		readonly IStorageProvider _provider;
		readonly DocumentStore _documentStore;
		readonly RelationStorage _relationStorage;

		Session _session;
		IProviderTransaction _transaction_;
		
		public ProviderStorage(IStorageProvider provider)
		{
			_provider = provider;
			var tables = new DocumentTables(provider);
			var serializer = new DocumentSerializer();
			
			_documentStore = new DocumentStore(_provider, tables, serializer);
			_session = new Session();
			_relationStorage = new RelationStorage(_provider, tables, serializer, _session);
		}

		public void Dispose()
		{
			_provider.Dispose();
		}

		#region Session

		public IDisposable beginSession()
		{
			return _session.begin();
		}

		#endregion

		#region Transactions

		public IDisposable beginTransaction()
		{
			if (_transaction_ != null)
				throw new Exception("A transaction is already active.");

			_transaction_ = _provider.beginTransaction();

			return new DisposeAction(() =>
				{
					Debug.Assert(_transaction_ != null);
					_transaction_.Dispose();
				});
		}

		public void commit()
		{
			if (_transaction_ == null)
				throw new Exception("No transaction active.");

			_transaction_.commit();
		}

		#endregion

		#region Documents

		public void store(object obj)
		{
			_documentStore.store(obj);
		}

		public IEnumerable<object> queryAll(Type t)
		{
			return _documentStore.queryAll(t);
		}

		public IEnumerable<DocumentT> queryByKey<DocumentT, KeyT>(
			Expression<Func<DocumentT, KeyT>> keyMember, string keyValue)
		{
			return _documentStore.queryByKey(keyMember, keyValue);
		}

		#endregion


		#region Relations

		public IRelation<FromT, ToT> relation<FromT, ToT>(string name)
		{
			return _relationStorage.createRelation<FromT, ToT>(name);
		}

		#endregion
	}
}
