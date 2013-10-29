using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RootSE;
using RootSE.ORM;
using RootSE.Provider;

namespace RootSETests
{
	[TestFixture]
	public class ORM
	{

		class TestName
		{
			public string A;
			public string B;
			public int C;
		}

		const string dbName = "test.db";
		const string TableName = "TableX";

		[Test]
		public void test()
		{
			if (File.Exists(dbName))
				File.Delete(dbName);

			const string AValue = "xyx";
			const string BValue = "dfslsdfjfsd";
			const int CValue = 33030;

			using (var provider = StorageProvider.openOrCreate(dbName, new StorageProviderOptions()))
			{
				provider.createTableAndIndicesForType<TestName>(TableName);

				var tn = new TestName {A = AValue, B = BValue, C = CValue};

				provider.insert(TableName, tn);

				var r = provider.query<TestName>(TableName);
				
				Assert.That(r.Count(), Is.EqualTo(1));
				var res = r.First();

				Assert.That(res.A, Is.EqualTo(AValue));
				Assert.That(res.B, Is.EqualTo(BValue));
				Assert.That(res.C, Is.EqualTo(CValue));
			}
		}

		class JsonTestAttributes
		{
			public string a;
			public int b;
		}

		class JsonTest
		{
			public JsonTestAttributes Attributes;
		}

		[Test]
		public void testImplicitJsonSerialization()
		{
			if (File.Exists(dbName))
				File.Delete(dbName);


			using (var provider = StorageProvider.openOrCreate(dbName, new StorageProviderOptions()))
			{
				provider.createTableAndIndicesForType<JsonTest>(TableName);

				var attr = new JsonTestAttributes
				{
					a = "TestString",
					b = 42
				};

				var tn = new JsonTest {Attributes = attr};
				
				provider.insert(TableName, tn);

				var r = provider.query<JsonTest>(TableName);

				var ri = r.Single();

				Assert.NotNull(ri.Attributes);
				var attr2 = ri.Attributes;
				Assert.That(attr2.a, Is.EqualTo(attr.a));
				Assert.That(attr2.b, Is.EqualTo(attr.b));
			}
		}

		sealed class NoPrimaryKey
		{
			public int a;
			[Unique] public int b;
			[Index] public int c;
		}

		sealed class PrimaryRowId
		{
			public int b;
			[RowId]
			public int primary;
			public int c;
		}

		sealed class PrimaryRowId2
		{
			public int b;
			[RowId]
			public int primary;
			[Unique, Index]
			public int c;
		}

		sealed class PrimaryUniqueIndex
		{
			public int b;
			[Unique, Index]
			public int primary;
			public int c;
		}

		sealed class NoPrimaryException
		{
			[Unique, Index] public int a;
			[Unique, Index] public int b;
		}


		[Test]
		public void testPrimaryKeySelection()
		{
			Assert.That(ORM<NoPrimaryKey>.PrimaryKeyIndex, Is.Null);
			Assert.That(ORM<PrimaryRowId>.PrimaryKeyColumnName_, Is.EqualTo("primary"));
			Assert.That(ORM<PrimaryUniqueIndex>.PrimaryKeyColumnName_, Is.EqualTo("primary"));
			Assert.That(ORM<PrimaryRowId2>.PrimaryKeyColumnName_, Is.EqualTo("primary"));
		}

		[Test, ExpectedException(typeof(TypeInitializationException))]
		public void testNoPrimaryException()
		{
			var name = ORM<NoPrimaryException>.PrimaryKeyColumnName_;
		}
	}
}

