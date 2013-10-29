using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RootSE.Provider;
using Toolbox.Constraints;

namespace RootSE.ORM
{
	public interface IRepository
	{
		void recreateTable();
		IEnumerable<KeyT> queryPrimaryKeys<KeyT>(Criteria criteria);
		void delete(object primaryKey);
		void delete(Criteria criteria);

		IProviderTransaction beginTransaction();
		IEnumerable<object> queryAll();
		object tryGet(object primaryKey);
		ulong queryCount();
	}

	[Pure]
	public sealed class Repository<InstanceT> : IRepository
		where InstanceT : class
	{
		readonly string PrimaryKeyColumnName;
		readonly IStorageProvider _storageProvider;

		public Repository(IStorageProvider storageProvider)
		{
			_storageProvider = storageProvider;

			var primaryIndex = ORM<InstanceT>.PrimaryKeyIndex;
			Debug.Assert(primaryIndex.HasValue);
			PrimaryKeyColumnName = ORM<InstanceT>.ColumnNames[primaryIndex.Value];

			_storageProvider.transact(() =>
				{
					if (!_storageProvider.hasTableForType<InstanceT>())
						_storageProvider.createTableAndIndicesForType<InstanceT>();
				});
		}

		public string TableName
		{
			get { return ORM<InstanceT>.TableName; }
		}

		public string PrimaryKeyColumn
		{
			get { return PrimaryKeyColumnName; }
		}

		public void recreateTable()
		{
			_storageProvider.transact(() =>
				{
					_storageProvider.deleteTable(ORM<InstanceT>.TableName);
					_storageProvider.createTableAndIndicesForType<InstanceT>();
				});
		}

		public IEnumerable<InstanceT> queryAll()
		{
			return _storageProvider.queryORM<InstanceT>();
		}

		IEnumerable<object> IRepository.queryAll()
		{
			return queryAll().Cast<object>();
		}

		public IEnumerable<KeyT> queryPrimaryKeys<KeyT>(Criteria criteria)
		{
			return _storageProvider.queryColumn<KeyT>(ORM<InstanceT>.TableName, PrimaryKeyColumnName, criteria);
		}

		object IRepository.tryGet(object primaryKey)
		{
			return tryGet(primaryKey);
		}

		public InstanceT tryGet(object primaryKey)
		{
			return _storageProvider.tryGetByPrimaryKey<InstanceT>(primaryKey);
		}

		public IEnumerable<InstanceT> query(Criteria criteria, OrderBy orderBy_ = null, Limit limit_ = null)
		{
			return _storageProvider.queryORM<InstanceT>(criteria, orderBy_, limit_);
		}

		public void insert(InstanceT instance)
		{
			_storageProvider.insert(instance);
		}

		public void update(InstanceT instance)
		{
			_storageProvider.update(instance);
		}

		public void delete(object primaryKey)
		{
			_storageProvider.delete<InstanceT>(new ColumnValue(PrimaryKeyColumnName, primaryKey));
		}

		public void delete(Criteria criteria)
		{
			_storageProvider.delete<InstanceT>(criteria);
		}

		public IProviderTransaction beginTransaction()
		{
			return _storageProvider.beginTransaction();
		}

		public ulong queryCount()
		{
			return _storageProvider.queryCount<InstanceT>();
		}
	}
}
