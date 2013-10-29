using RootSE.Engine;
using RootSE.Provider;

namespace RootSE
{
	public static class Storage
	{
		public static IStorage openOrCreate(string dbName, StorageProviderOptions options)
		{
			var provider = StorageProvider.openOrCreate(dbName, options);
			return new ProviderStorage(provider);
		}
	}
}
