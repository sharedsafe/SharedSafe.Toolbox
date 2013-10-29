using System.Collections.Generic;

namespace DomainModeling
{
	sealed class DomainModelBuilderEventDistributor
	{
		readonly IEventDistributor _eventDistributor;

		public DomainModelBuilderEventDistributor(IEnumerable<IDomainModelBuilder> builders)
		{
			_eventDistributor = DomainModelingTools.createEventDistributor(builders);
		}

		public IEventDistributionSession beginSession()
		{
			return _eventDistributor.beginSession();
		}
	}
}
