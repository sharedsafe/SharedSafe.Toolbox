using System;

namespace DomainModeling
{
	public interface IDomainEventsSpecification
	{
		Type TypeOfFirstEvent { get; }
	}
}
