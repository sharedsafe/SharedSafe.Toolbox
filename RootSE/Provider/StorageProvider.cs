using RootSE.Provider.SQLite;

namespace RootSE.Provider
{
	public static class StorageProvider
	{
		public static IStorageProvider openOrCreate(string dbName, StorageProviderOptions options)
		{
			return new SQLiteProvider(dbName, true, options);
		}

		public static IStorageProvider open(string dbName, StorageProviderOptions options)
		{
			return new SQLiteProvider(dbName, false, options);
		}
	}
}
