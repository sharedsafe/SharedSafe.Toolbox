using System;
using System.Collections.Generic;
using System.Linq;
using RootSE.Provider;

namespace DomainModeling.Storage
{
	sealed class ReferrerResolver
	{
		readonly DomainModelRegistry _modelRegistry;
		readonly DomainRepositories _domainRepositories;

		public ReferrerResolver(DomainModelRegistry modelRegistry, DomainRepositories domainRepositories)
		{
			_modelRegistry = modelRegistry;
			_domainRepositories = domainRepositories;
		}

		public IEnumerable<Reference> resolveReferrers(Reference reference)
		{
			var referrers = new List<Reference>();

			foreach (var referrer in _modelRegistry.DomainTypes[reference.Type].Referrers)
			{
				var r = referrer;
				var repository = _domainRepositories.getFor(r.Type);
				var keys = repository.queryPrimaryKeys<Guid>(Term.column(r.Member).equals(reference.Id));
				var references = from k in keys select new Reference(r.Type, k);
				referrers.AddRange(references);
			}

			return referrers;
		}
	}
}
