using System;

namespace DomainModeling
{
	public interface IDocumentMetadataProvider
	{
		DateTime? tryQueryMostRecentChange();
		bool queryIsBlank();
	}
}
