using Konstruktor;
using RootSE.ORM;
using RootSE.Provider;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class ReplayService : IReplayService
	{
		readonly LocalDataStore _dataStore;
		readonly DomainRepositories _repositories;
		readonly Repository<ChangeSet> _changeSets;
		readonly Repository<CompensatingChangeSet> _compensatingChangeSets;
		readonly DomainEventRegistry _eventRegistry;
		readonly ChangeSetWriter _changeSetWriter;
		readonly IDomainModelBootstrapper _bootstrapper;
		readonly DomainViewBuilderObjectDistributor _objectViewDistributor;

		public ReplayService(
			LocalDataStore dataStore, 
			DomainRepositories repositories, 
			Repository<ChangeSet> changeSets,
			Repository<CompensatingChangeSet> compensatingChangeSets,
			DomainEventRegistry eventRegistry,
			ChangeSetWriter changeSetWriter,
			IDomainModelBootstrapper bootstrapper,
			DomainViewBuilderObjectDistributor objectViewDistributor)
		{
			_dataStore = dataStore;
			_repositories = repositories;
			_changeSets = changeSets;
			_compensatingChangeSets = compensatingChangeSets;
			_eventRegistry = eventRegistry;
			_changeSetWriter = changeSetWriter;
			_bootstrapper = bootstrapper;
			_objectViewDistributor = objectViewDistributor;
		}

		public void replayAll()
		{
			_dataStore.transact(replayAllTransaction);
		}

		/**
			Replaying is different for performance reasons, 
			it first applies all changesets to the compensating changeset generator and to the model.

			Then it bootstraps the model and applies the resulting events to the views.
		**/

		void replayAllTransaction()
		{
			_repositories.recreateTables();
			_compensatingChangeSets.delete(Criteria.All);

			using (_eventRegistry.makeCurrent())
			{
				foreach (var changeSet in _changeSets.queryAll())
				{
					_changeSetWriter.applyToCCSAndModel(changeSet);
				}

				using (var session = _objectViewDistributor.beginSession())
					_bootstrapper.bootstrapModel(session.distribute);
			}
		}
	}
}
