using System;
using System.Collections.Generic;

namespace DomainModeling
{
	public interface IChangeSetWriter
	{
		// architecture: don't return anything here!
		IEnumerable<IDomainEvent> storeIntentionalChangesAndUpdateViews(IEnumerable<IDomainEvent> intentionalEvents);

		event Action ChangeSetWritten;
	}
}