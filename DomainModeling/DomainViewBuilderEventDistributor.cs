using System.Collections.Generic;
using System.Linq;

namespace DomainModeling
{
	sealed class DomainViewBuilderEventDistributor
	{
		readonly IEventDistributor _eventDistributor;

		public DomainViewBuilderEventDistributor(IEnumerable<IDomainViewBuilder> builders)
		{
			// cast required by IOS
			_eventDistributor = DomainModelingTools.createEventDistributor(builders.Cast<object>());
		}

		public IEventDistributionSession beginSession()
		{
			return _eventDistributor.beginSession();
		}
	}
}
