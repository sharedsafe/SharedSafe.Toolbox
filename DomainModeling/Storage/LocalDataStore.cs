using System;
using System.Diagnostics;
using RootSE.Provider;

namespace DomainModeling.Storage
{
	public sealed class LocalDataStore
	{
		readonly IStorageProvider _provider;

		public LocalDataStore(IStorageProvider provider)
		{
			_provider = provider;
		}

		[DebuggerStepThrough]
		public void transact(Action action)
		{
			using (var t = _provider.beginTransaction())
			{
				action();
				t.commit();
			}
		}

		public bool IsInTransaction
		{
			get { return _provider.IsTransacting; }
		}
	}
}
