using System;
using System.Linq;
using Konstruktor;
using RootSE.ORM;
using RootSE.Provider;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class DocumentMetadataProvider : IDocumentMetadataProvider
	{
		readonly Repository<ChangeSet> _changeSets;

		public DocumentMetadataProvider(Repository<ChangeSet> changeSets)
		{
			_changeSets = changeSets;
		}

		public DateTime? tryQueryMostRecentChange()
		{
			var primaryIdColumn = _changeSets.PrimaryKeyColumn;

			var cs_ = _changeSets.query(
				Criteria.All,
				OrderBy.column(primaryIdColumn).Descending,
				Limit.One).SingleOrDefault();

			if (cs_ == null)
				return null;
			return new DateTime(cs_.Date, DateTimeKind.Utc);
		}

		public bool queryIsBlank()
		{
			return _changeSets.queryCount() == 0;
		}
	}
}
