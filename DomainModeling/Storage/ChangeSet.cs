using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RootSE.ORM;

namespace DomainModeling.Storage
{
	[Obfuscation]
	sealed class ChangeSet
	{
		[RowId] public ulong Id;
		[Index] public Guid Guid;

		public long Date;

		public DomainEvent[] Events;

		public static ChangeSet create(Guid guid, DateTime date, DomainEvent[] events)
		{
			Debug.Assert(date.Kind == DateTimeKind.Utc);

			return new ChangeSet
			{
				Guid = guid,
				Date = date.Ticks,
				Events = events
			};
		}
	}

	static class ChangeSetExtensions
	{
		public static IEnumerable<IDomainEvent> domainEventsOf(this ChangeSet cs)
		{
			return from e in cs.Events select e.Event;
		}
	}
}

