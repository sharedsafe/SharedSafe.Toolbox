using System.IO;
using System.Linq;
using NUnit.Framework;
using RootSE;
using RootSE.Provider;
using Toolbox;

namespace RootSETests
{
	[TestFixture]
	public sealed class RTree
	{
		IStorageProvider useEmptyDB()
		{
			const string dbName = "test.db";
			if (File.Exists(dbName))
				File.Delete(dbName);


			return StorageProvider.openOrCreate(dbName, new StorageProviderOptions());
		}

		const string IndexName = "index";

		[Test]
		public void createAndDeleteIndex()
		{

			using (var provider = useEmptyDB())
			{
				var rtree = provider.RTree;

				Assert.False(rtree.hasIndex(IndexName));
				rtree.createIndex(IndexName, 4);
				Assert.True(rtree.hasIndex(IndexName));
				rtree.deleteIndex(IndexName);
				Assert.False(rtree.hasIndex(IndexName));
			}
		}

		[Test]
		public void indexInsertUpdateAndDelete()
		{
			using (var provider = useEmptyDB())
			{
				var rtree = provider.RTree;
				rtree.createIndex(IndexName, 2);

				rtree.insert(IndexName, 0, Range.make(1.1f, 2.1f), Range.make(0.5f, 0.6f));

				rtree.insert(IndexName, 1, Range.make(1.1f, 2.1f), Range.make(0.5f, 0.6f));
				
				rtree.insert(IndexName, 2, Range.make(1.5f, 2.1f), Range.make(0.5f, 0.6f));
				rtree.update(IndexName, 1, Range.make(2.0f, 3.0f), Range.make(0.1f, 0.2f));

				rtree.delete(IndexName, 0);

				{
					var r = rtree.queryOverlapped(IndexName, Range.make(2.1f, 2.5f), Range.make(0.1f, 0.11f));
					var cnt = r.Count();
					Assert.That(cnt, Is.EqualTo(1));
				}

				{
					var r = rtree.queryOverlapped(IndexName, Range.make(2.0f, 2.5f), Range.make(0.1f, 0.55f));
					var cnt = r.Count();
					Assert.That(cnt, Is.EqualTo(2));
				}

				rtree.delete(IndexName, 1);
				rtree.delete(IndexName, 2);
			}
		}
	}
}
