using System.Collections.Generic;

namespace DomainModeling
{
	public interface ITransformEvent<in EventT>
		where EventT : IDomainEvent
	{
		IEnumerable<IDomainEvent> transform(EventT input);
	}
}
