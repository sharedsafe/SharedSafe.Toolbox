using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RootSE.ORM;

namespace DomainModeling.Storage
{
	[Obfuscation]
	sealed class CompensatingChangeSet
	{
		[Unique, Index]
		public Guid Id;

		public DomainEvent[] Events;
	}

	static class CompensatingChangeSetExtensions
	{
		public static IEnumerable<IDomainEvent> domainEventsOf(this CompensatingChangeSet cs)
		{
			return from e in cs.Events select e.Event;
		}
	}
}
