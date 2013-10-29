using System;

namespace DomainModeling
{
	public interface IHandleEventTransactions
	{
		IDisposable beginEventTransaction();
	}
}
