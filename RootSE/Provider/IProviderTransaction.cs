using System;

namespace RootSE.Provider
{
	public interface IProviderTransaction : IDisposable
	{
		void commit();
	}
}