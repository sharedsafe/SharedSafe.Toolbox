using System.Linq;
using NUnit.Framework;
using RootSE;

namespace RootSETests
{
	[TestFixture]
	public class Basics : TestBase
	{
		[Test]
		public void openDatabase()
		{
			createNew().Dispose();
		}

		sealed class TestObj
		{
			public string Name;
		}


		[Test]
		public void store100kObjects()
		{
			using (var storage = createNew())
			{
				using (storage.beginTransaction())
				{
					for (var i = 0; i != 100000; ++i)
						storage.store(new TestObj {Name = "Test" + i});

					storage.commit();
				}
			}
		}

		[Test]
		public void storeAndRetrieve()
		{
			using (var storage = createNew())
			{
				storage.store(new TestObj { Name = "test"});
				var all = storage.queryAll<TestObj>().ToArray();
				Assert.That(all.Count(), Is.EqualTo(1));
				Assert.That(all.First().Name, Is.EqualTo("test"));
			}
		}

		[Test]
		public void testEscape()
		{
			using (var storage = createNew())
			{
				const string EscapeStringTest = "test''test'";
				storage.store(new TestObj { Name=EscapeStringTest});
				var all = storage.queryAll<TestObj>().ToArray();

				Assert.That(all.Count(), Is.EqualTo(1));
				Assert.That(all.First().Name, Is.EqualTo(EscapeStringTest));
			}
		}

		[Test]
		public void testUTFEncoding()
		{
			const string EncodedTest = "一言も言わずに（ひとこともいわずに） / without a word, in silence ☆＝無言で（むごんで）";

			using (var storage = createNew())
			{
				storage.store(new TestObj { Name = EncodedTest });
				var all = storage.queryAll<TestObj>().ToArray();

				Assert.That(all.Count(), Is.EqualTo(1));
				Assert.That(all.First().Name, Is.EqualTo(EncodedTest));
			}
		}
	}
}
