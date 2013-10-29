using System.Collections.Generic;
using System.Linq;
using Toolbox.Algorithms;

namespace DomainModeling.Storage
{
	/**
		A changeset gets normalized by prefixing it with destructive events that destroy all referring objects.

		A changeset normalization must run inside the transaction.
	**/

	sealed class ChangeSetDenormalizer
	{
		readonly DomainModelRegistry _modelRegistry;
		readonly ReferrerResolver _referrerResolver;

		public ChangeSetDenormalizer(DomainModelRegistry modelRegistry, ReferrerResolver referrerResolver)
		{
			_modelRegistry = modelRegistry;
			_referrerResolver = referrerResolver;
		}

		public ChangeSet denormalize(ChangeSet intentionalChangeSet)
		{
			var domainEvents = from e in intentionalChangeSet.Events select e.Event;
			var normalizedEvents = denormalize(domainEvents);

			return new ChangeSet
			{
				Guid = intentionalChangeSet.Guid,
				Date = intentionalChangeSet.Date,
				Events = normalizedEvents.Select(de => new DomainEvent { Event = de }).ToArray()
			};
		}

		IEnumerable<IDomainEvent> denormalize(IEnumerable<IDomainEvent> events)
		{
			var destructiveEventsOfDependencies = resolveAllDestructiveDependencies(events)
				.Select(_modelRegistry.createDestructiveEventByReference);

			return destructiveEventsOfDependencies.Concat(events);
		}

		IEnumerable<Reference> resolveAllDestructiveDependencies(IEnumerable<IDomainEvent> domainEvents)
		{
			// first find all roots of the dependencies (we don't want to original objects to be included).
			// the original domain events stay as they are.

			var dependencyRoots = new List<Reference>();

			foreach (var ev in domainEvents)
			{
				var destructive = ev as IDestructiveDomainEvent;
				if (destructive == null)
					continue;

				var reference = toReference(destructive);
				var immediateDependencies = _referrerResolver.resolveReferrers(reference);
				dependencyRoots.AddRange(immediateDependencies);
			}

			var topologicallySortedDependencies = dependencyRoots
				.sortTopologicallyReverse(reference => 
					_referrerResolver.resolveReferrers(reference));

			return topologicallySortedDependencies;
		}

		Reference toReference(IDestructiveDomainEvent destructiveDomainEvent)
		{
			var domainType = _modelRegistry.DestructiveEventTypes[destructiveDomainEvent.GetType()].DomainType;
			return new Reference(domainType, destructiveDomainEvent.Id);
		}
	}
}
