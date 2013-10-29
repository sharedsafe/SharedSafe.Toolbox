using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Konstruktor;
using RootSE.ORM;
using Toolbox;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class ChangeSetWriter : IChangeSetWriter
	{
		readonly LocalDataStore _localDataStore;
		readonly ChangeSetDenormalizer _denormalizer;
		readonly Repository<ChangeSet> _changeSets;
		readonly DomainEventRegistry _eventRegistry;
		readonly CompensatingChangeSetWriter _compensatingChangeSetWriter;
		readonly DomainModelBuilderEventDistributor _domainModelBuilderEventDistributor;
		readonly DomainViewBuilderEventDistributor _domainViewBuilderEventDistributor;
		readonly RedoStack _redoStack;

		public ChangeSetWriter(
			LocalDataStore localDataStore, 
			ChangeSetDenormalizer denormalizer,
			Repository<ChangeSet> changeSets,
			DomainEventRegistry eventRegistry,
			CompensatingChangeSetWriter compensatingChangeSetWriter,
	
			DomainModelBuilderEventDistributor domainModelBuilderEventDistributor,
			DomainViewBuilderEventDistributor domainViewBuilderEventDistributor,

			RedoStack redoStack)
		{
			_changeSets = changeSets;
			_eventRegistry = eventRegistry;
			_compensatingChangeSetWriter = compensatingChangeSetWriter;
			
			_domainModelBuilderEventDistributor = domainModelBuilderEventDistributor;
			_domainViewBuilderEventDistributor = domainViewBuilderEventDistributor;

			_redoStack = redoStack;
			_localDataStore = localDataStore;
			_denormalizer = denormalizer;
		}

		public event Action ChangeSetWritten;

		// Stores and returns the (normalized) domain events.

		public IEnumerable<IDomainEvent> storeIntentionalChangesAndUpdateViews(IEnumerable<IDomainEvent> intentionalEvents)
		{
			// any attempt to store new events clears the redo stack.

			_redoStack.clear();

			return storeAndUpdateViews(intentionalEvents);
		}

		internal IEnumerable<IDomainEvent> storeAndUpdateViews(IEnumerable<IDomainEvent> intentionalEvents)
		{
			IEnumerable<IDomainEvent> denormalizedEvents = null;

			using (_eventRegistry.makeCurrent())
			{
				_localDataStore.transact(() =>
				{
					var guid = Guid.NewGuid();
					var intentionalChangeSet = ChangeSet.create(guid,
						DateTime.UtcNow,
						intentionalEvents.Select(de => new DomainEvent { Event = de }).ToArray());

					_changeSets.insert(intentionalChangeSet);

					var denormalizedChangeSet = applyChangeSet(intentionalChangeSet);
					denormalizedEvents = denormalizedChangeSet.domainEventsOf();
				});

				ChangeSetWritten.raise();
			}

			return denormalizedEvents;
		}

		internal ChangeSet applyChangeSet(ChangeSet intentionalChangeSet)
		{
			Debug.Assert(_localDataStore.IsInTransaction);
			var denormalizedChangeSet = _denormalizer.denormalize(intentionalChangeSet);

			using (var ccsWriteSession = _compensatingChangeSetWriter.beginTransaction(denormalizedChangeSet))
			using (var modelDistributionSession = _domainModelBuilderEventDistributor.beginSession())
			using (var viewDistributionSession = _domainViewBuilderEventDistributor.beginSession())
			{
				foreach (var ev in denormalizedChangeSet.domainEventsOf())
				{
					ccsWriteSession.generateAndStoreFor(ev);
					modelDistributionSession.distribute(ev);
					viewDistributionSession.distribute(ev);
				}
			}
			
			return denormalizedChangeSet;
		}

		internal void applyToCCSAndModel(ChangeSet intentionalChangeSet)
		{
			Debug.Assert(_localDataStore.IsInTransaction);
			var denormalizedChangeSet = _denormalizer.denormalize(intentionalChangeSet);

			using (var ccsWriteSession = _compensatingChangeSetWriter.beginTransaction(denormalizedChangeSet))
			using (var modelDistributionSession = _domainModelBuilderEventDistributor.beginSession())
			{
				foreach (var ev in denormalizedChangeSet.domainEventsOf())
				{
					ccsWriteSession.generateAndStoreFor(ev);
					modelDistributionSession.distribute(ev);
				}
			}
		}

		internal void applyToModelAndViews(IEnumerable<IDomainEvent> denormalized)
		{
			using (var modelSession = _domainModelBuilderEventDistributor.beginSession())
			using (var viewSession = _domainViewBuilderEventDistributor.beginSession())
			{
				foreach (var ev in denormalized)
				{
					modelSession.distribute(ev);
					viewSession.distribute(ev);
				}
			}
		}

		internal void applyToViews(IEnumerable<IDomainEvent> denormalized)
		{
			using (var viewSession = _domainViewBuilderEventDistributor.beginSession())
				foreach (var ev in denormalized)
					viewSession.distribute(ev);
		}
	}
}
