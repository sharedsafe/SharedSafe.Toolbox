using System;

namespace RootSE
{
	public interface IStorageTransaction : IDisposable
	{
		void commit();
	}
}
