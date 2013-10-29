using System;

namespace DomainModeling
{
	public interface IDestructiveDomainEvent : IDomainEvent
	{
		Guid Id { get; set; }
	}
}
