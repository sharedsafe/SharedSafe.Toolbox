using System;
using System.Collections.Generic;
using DomainModeling.Meta;
using DomainModeling.Storage;
using Toolbox;

namespace DomainModeling.Tools
{
	sealed class Individualization : IIndividualization
	{
		readonly DomainModelRegistry _registry;
		readonly Dictionary<Guid, Guid> _convertedGuids = new Dictionary<Guid, Guid>();

		public Individualization(DomainModelRegistry registry)
		{
			_registry = registry;
		}

		public void Dispose()
		{
		}

		public IEnumerable<DomainT> individualize<DomainT>(IEnumerable<DomainT> sources)
			where DomainT : IDomainObject
		{
			var t = typeof (DomainT);

			foreach (var source in sources)
			{
				var accessors = _registry.GuidAccessorsByType[t];
				yield return individualize(source, accessors);
			}
		}

		public IEnumerable<IDomainObject> individualize(IEnumerable<IDomainObject> domainObjects)
		{
			foreach (var obj in domainObjects)
			{
				var accesors = _registry.GuidAccessorsByType[obj.GetType()];
				yield return individualize(obj, accesors);
			}
		}

		DomainT individualize<DomainT>(DomainT source, IEnumerable<GuidAccessor> accessors)
			where DomainT : IDomainObject
		{
			var target = source.memberwiseClone();
			foreach (var accessor in accessors)
			{
				convertGuid(source, target, accessor);
			}

			return target;
		}

		void convertGuid(IDomainObject source, IDomainObject target, GuidAccessor accessor)
		{
			var sourceId = accessor.get(source);
			var targetId = convert(sourceId);
			accessor.set(target, targetId);
		}

		Guid convert(Guid from)
		{
			Guid to;
			if (!_convertedGuids.TryGetValue(from, out to))
			{
				to = Guid.NewGuid();
				_convertedGuids.Add(from, to);
			}
			return to;
		}
	}
}
