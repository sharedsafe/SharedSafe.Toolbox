using System.Collections.Generic;
using System.Linq;

namespace DomainModeling
{
	public interface IDomainObject
	{
		IEnumerable<IDomainEvent> bootstrap();
	}

	public static class IDomainObjectExtensions
	{
		public static IEnumerable<IDomainEvent> bootstrap(this IEnumerable<IDomainObject> objects)
		{
			return objects.Select(obj => obj.bootstrap()).SelectMany(objs => objs);
		}
	}
}
