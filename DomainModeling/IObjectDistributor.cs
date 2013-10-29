using System;

namespace DomainModeling
{
	public interface IObjectDistributor
	{
		IObjectDistributionSession beginSession();
	}

	public interface IObjectDistributionSession : IDisposable
	{
		void distribute(IDomainObject obj);
	}
}