using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DomainModeling.Detail;
using RootSE.ORM;

namespace DomainModeling.Storage
{
	sealed class CompensatingChangeSetWriter
	{
		readonly Repository<CompensatingChangeSet> _compensatingChangeSets;
		readonly ICompensatingChangeSetGenerator _generator;

		public CompensatingChangeSetWriter(
			Repository<CompensatingChangeSet> compensatingChangeSets, 
			ICompensatingChangeSetGenerator generator)
		{
			_compensatingChangeSets = compensatingChangeSets;
			_generator = generator;
		}

		public CCSWriteTransaction beginTransaction(ChangeSet changeSet)
		{
			return new CCSWriteTransaction(changeSet, _generator, _compensatingChangeSets);
		}

		public sealed class CCSWriteTransaction : IDisposable
		{
			readonly ChangeSet _changeSet;
			readonly ICompensatingChangeSetGenerator _generator;
			readonly Repository<CompensatingChangeSet> _compensatingChangeSets;

			readonly List<IDomainEvent[]> _compensatingEventGroups = new List<IDomainEvent[]>();

			public CCSWriteTransaction(
				ChangeSet changeSet, 
				ICompensatingChangeSetGenerator generator, Repository<CompensatingChangeSet> compensatingChangeSets)
			{
				_changeSet = changeSet;
				_generator = generator;
				_compensatingChangeSets = compensatingChangeSets;
			}

			public void generateAndStoreFor(IDomainEvent domainEvent)
			{
				var compensatingEvents = EventDispatcher.transform(_generator, domainEvent).ToArray();
				_compensatingEventGroups.Add(compensatingEvents);
			}

			public void Dispose()
			{
				var query = _compensatingEventGroups
					.ToArray()
					.Reverse() // <warning: don't run this on the List.
					.SelectMany(ev => ev)
					.Select(ev => new DomainEvent {Event = ev});

				var compensatingEvents = query.ToArray();

				var ccs = new CompensatingChangeSet {Id = _changeSet.Guid, Events = compensatingEvents};

				_compensatingChangeSets.insert(ccs);
			}
		}
	}
}
