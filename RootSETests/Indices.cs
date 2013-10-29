using System.Linq;
using NUnit.Framework;
using RootSE.Provider;

namespace RootSETests
{
	[TestFixture]
	public class Indices : TestBase
	{
		sealed class Doc
		{
			public string Name;
			public string Value;
		}

		[Test]
		public void simpleTest()
		{
			using (var storage = createNew())
			using (storage.beginTransaction())
			{
				storage.store(new Doc { Name = "Hello", Value = "World1" });
				storage.store(new Doc { Name = "Hello3", Value = "World2" });
				storage.store(new Doc { Name = "_NoHello3", Value = "World3" });

				var r = storage.queryByKey((Doc doc) => doc.Name, "Hello").ToArray();

				Assert.That(r.Length, Is.EqualTo(1));
				Assert.That(r[0].Name, Is.EqualTo("Hello"));
				Assert.That(r[0].Value, Is.EqualTo("World1"));
			}
		}

	}
}
