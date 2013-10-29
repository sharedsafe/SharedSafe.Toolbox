using System.Collections.Generic;
using System.Linq;
using DomainModeling.Storage;

namespace DomainModeling
{
	public interface IEventTransaction
	{
		IDomainEvent[] Events { get; }
	}

	public static class IDomainEventExtensions
	{
		public static IEventTransaction toTransaction(this IEnumerable<IDomainEvent> events)
		{
			return new EventTransaction(events.ToArray());
		}

		public static IEventTransaction toTransaction(this IDomainEvent ev)
		{
			return new EventTransaction(new[]{ev});
		}

		internal static IEventTransaction toTransaction(this ChangeSet cs)
		{
			return cs.domainEventsOf().toTransaction();
		}

		sealed class EventTransaction : IEventTransaction
		{
			public IDomainEvent[] Events { get; private set; }

			public EventTransaction(IDomainEvent[] events)
			{
				Events = events;
			}
		}
	}
}
