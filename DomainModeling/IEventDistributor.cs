using System;

namespace DomainModeling
{
	public interface IEventDistributor
	{
		IEventDistributionSession beginSession();
	}

	public interface IEventDistributionSession : IDisposable
	{
		void distribute(IDomainEvent ev);
	}
}