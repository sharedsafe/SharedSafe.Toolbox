using System.IO;
using RootSE;
using RootSE.Provider;

namespace RootSETests
{
	public class TestBase
	{
		const string TestDB = "test.db";

		protected static IStorage createNew()
		{
			if (File.Exists(TestDB))
				File.Delete(TestDB);

			return Storage.openOrCreate(TestDB, new StorageProviderOptions());
		}

		protected static IStorageProvider createNewDBAndStorageProvider()
		{
			if (File.Exists(TestDB))
				File.Delete(TestDB);

			return StorageProvider.openOrCreate(TestDB, new StorageProviderOptions());
		}
	}
}
