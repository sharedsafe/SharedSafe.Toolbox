using System.Collections.Generic;
using DomainModeling.Detail;

namespace DomainModeling
{
	public static class DomainModelingTools
	{
		public static IEventDistributor createEventDistributor(IEnumerable<object> targets)
		{
			return new EventDistributor(targets);
		}

		public static IObjectDistributor createObjectDistributor(IEnumerable<object> targets)
		{
			return new ObjectDistributor(targets);
		}
	}
}
