using System;
using System.Collections.Generic;
using RootSE.ORM;
using DomainModeling.Storage;

namespace DomainModeling
{
	public sealed class DomainRepositories
	{
		readonly IDictionary<Type, IRepository> _repositories;
		readonly LocalDataStore _localDataStore;

		public DomainRepositories(LocalDataStore localDataStore, IDictionary<Type, IRepository> repositories)
		{
			_repositories = repositories;
			_localDataStore = localDataStore;
		}

		public IRepository getFor<TypeT>()
		{
			return getFor(typeof (TypeT));
		}

		public IRepository getFor(Type t)
		{
			return _repositories[t];
		}

		public void recreateTables()
		{
			_localDataStore.transact(() =>
				{
					foreach (var repository in _repositories.Values)
						repository.recreateTable();
				});
		}
	}
}
