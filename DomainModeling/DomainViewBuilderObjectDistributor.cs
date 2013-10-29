using System.Collections.Generic;
using System.Linq;

namespace DomainModeling
{
	sealed class DomainViewBuilderObjectDistributor
	{
		readonly IObjectDistributor _objectDistributor;

		public DomainViewBuilderObjectDistributor(IEnumerable<IDomainViewBuilder> builders)
		{
			// cast required for IOS
			_objectDistributor = DomainModelingTools.createObjectDistributor(builders.Cast<object>());
		}

		public IObjectDistributionSession beginSession()
		{
			return _objectDistributor.beginSession();
		}
	}
}
