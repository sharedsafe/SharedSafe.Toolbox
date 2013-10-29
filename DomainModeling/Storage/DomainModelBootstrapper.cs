using System;
using Konstruktor;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class DomainModelBootstrapper : IDomainModelBootstrapper
	{
		readonly LocalDataStore _store;
		readonly DomainModelRegistry _registry;
		readonly DomainRepositories _repositories;

		public DomainModelBootstrapper(LocalDataStore store, DomainModelRegistry registry, DomainRepositories repositories)
		{
			_store = store;
			_registry = registry;
			_repositories = repositories;
		}

		public void bootstrapModel(Action<IDomainObject> dispatch)
		{
			_store.transact(() => internalBootstrapModel(dispatch));
		}

		void internalBootstrapModel(Action<IDomainObject> dispatch)
		{
			foreach (var domainType in _registry.TopologicallySortedDomainTypes)
			{
				var repository = _repositories.getFor(domainType);
				foreach (IDomainObject domainObject in repository.queryAll())
					dispatch(domainObject);
			}
		}
	}
}