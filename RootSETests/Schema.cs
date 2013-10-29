using System;
using NUnit.Framework;
using RootSE.ORM;
using RootSE.Provider;
using Toolbox;

namespace RootSETests
{
	namespace Before
	{
		sealed class ColumnsReordered
		{
			public Guid Test;
			public int aa;
		}

		sealed class SameRowId
		{
			[RowId]
			public long aa;
		}

		sealed class SameUniqueIndex
		{
			[Unique, Index]
			public long aa;
		}

		sealed class AllFeaturesUnchanged
		{
			public long aa;
			public Guid bb;
			[Index]
			public long cc;
			[RowId]
			public long ee;
			[Null]
			public string ff;
		}

	
		sealed class AllFeaturesUniqueUnchanged
		{
			public long aa;
			public Guid bb;
			[Index, Unique]
			public long cc;
			public long ee;
			[Null]
			public string ff;
		}

		sealed class TypeChanged
		{
			public string aa;
		}

		sealed class NewInserted
		{
			public int aa;
		}
		
		sealed class Removed
		{
			public int aa;
			public int bb;
		}

		sealed class IndexAdded
		{
			public int aa;
		}

		sealed class IndexRemoved
		{
			[Index]
			public int aa;
		}

		sealed class UniqueIndexAdded
		{
			[Index]
			public int aa;
		}

		sealed class UniqueIndexRemoved
		{
			[Index, Unique]
			public int aa;
		}

		sealed class RowIdAdded
		{
			public long aa;
		}

		sealed class RowIdRemoved
		{
			[RowId]
			public long aa;
		}

		sealed class RowIdChangedMember
		{
			[RowId]
			public long aa;
			public long bb;
		}

	}

	namespace After
	{
		sealed class ColumnsReordered
		{
			public int aa;
			public Guid Test;
		}


		sealed class SameRowId
		{
			[RowId]
			public long aa;
		}

		sealed class SameUniqueIndex
		{
			[Unique, Index]
			public long aa;
		}

		sealed class AllFeaturesUnchanged
		{
			public long aa;
			public Guid bb;
			[Index]
			public long cc;
			[RowId]
			public long ee;
			[Null] 
			public string ff;
		}

		sealed class AllFeaturesUniqueUnchanged
		{
			public long aa;
			public Guid bb;
			[Index, Unique]
			public long cc;
			public long ee;
			[Null]
			public string ff;
		}

		sealed class TypeChanged
		{
			public int aa;
		}

		sealed class NewInserted
		{
			public int bb;
			public int aa;
		}

		sealed class Removed
		{
			public int aa;
		}

		sealed class IndexAdded
		{
			[Index]
			public int aa;
		}

		sealed class IndexRemoved
		{
			public int aa;
		}

		sealed class UniqueIndexAdded
		{
			[Index, Unique]
			public int aa;
		}

		sealed class UniqueIndexRemoved
		{
			[Index]
			public int aa;
		}

		sealed class RowIdAdded
		{
			[RowId]
			public long aa;
		}

		sealed class RowIdRemoved
		{
			public long aa;
		}


		sealed class RowIdChangedMember
		{
			public long aa;
			[RowId]
			public long bb;
		}

	}
	
	[TestFixture]
	public class Schema : TestBase
	{
		[Test]
		public void IndexEquality()
		{
			var a = new Index(new [] {"a", "b"}, true);
			var b = new Index(new [] {"a", "b"}, true);

			Assert.True(a.Equals(b));
		}

		[Test]
		public void IndexReorderedInequality()
		{
			var a = new Index(new[] { "a", "b" }, true);
			var b = new Index(new[] { "b", "a" }, true);

			Assert.False(a.Equals(b));
		}

		[Test]
		public void ColumnEquality()
		{
			var a = new Column("a", "b", false, false);
			var b = new Column("a", "b", false, false);

			Assert.True(a.Equals(b));
		}

		[Test]
		public void ColumnInEquality()
		{
			var a = new Column("a", "b", false, false);
			var b = new Column("a", "c", false, false);

			Assert.False(a.Equals(b));
		}

		[Test]
		public void ColumnInEquality2()
		{
			var a = new Column("a", "a", false, false);
			var b = new Column("a", "a", true, false);

			Assert.False(a.Equals(b));
		}

		[Test]
		public void ColumnInEquality3()
		{
			var a = new Column("a", "a", false, false);
			var b = new Column("a", "a", false, true);

			Assert.False(a.Equals(b));
		}

		[Test]
		public void columnsReordered()
		{
			requireCompatible<Before.ColumnsReordered, After.ColumnsReordered>();
		}

		[Test]
		public void sameRowId()
		{
			requireCompatible<Before.SameRowId, After.SameRowId>();
		}

		[Test]
		public void sameUniqueIndex()
		{
			requireCompatible<Before.SameUniqueIndex, After.SameUniqueIndex>();
		}

		[Test]
		public void allFeaturesUnchanged()
		{
			requireCompatible<Before.AllFeaturesUnchanged, After.AllFeaturesUnchanged>();
		}

		[Test]
		public void allFeaturesUniqueUnchanged()
		{
			requireCompatible<Before.AllFeaturesUniqueUnchanged, After.AllFeaturesUniqueUnchanged>();
		}

		[Test]
		public void typeChanged()
		{
			requireIncompatible<Before.TypeChanged, After.TypeChanged>();
		}

		[Test]
		public void newInserted()
		{
			requireIncompatible<Before.NewInserted, After.NewInserted>();
		}

		[Test]
		public void removed()
		{
			requireIncompatible<Before.Removed, After.Removed>();
		}

		[Test]
		public void indexAdded()
		{
			requireIncompatible<Before.IndexAdded, After.IndexAdded>();
		}

		[Test]
		public void indexRemoved()
		{
			requireIncompatible<Before.IndexRemoved, After.IndexRemoved>();
		}

		[Test]
		public void uniqueIndexAdded()
		{
			requireIncompatible<Before.UniqueIndexAdded, After.UniqueIndexAdded>();
		}

		[Test]
		public void uniqueIndexRemoved()
		{
			requireIncompatible<Before.UniqueIndexRemoved, After.UniqueIndexRemoved>();
		}

		[Test]
		public void rowIdAdded()
		{
			requireIncompatible<Before.RowIdAdded, After.RowIdAdded>();
		}

		[Test]
		public void rowIdRemoved()
		{
			requireIncompatible<Before.RowIdRemoved, After.RowIdRemoved>();
		}

		[Test]
		public void rowIdChangedMember()
		{
			requireIncompatible<Before.RowIdChangedMember, After.RowIdChangedMember>();
		}

		void requireIncompatible<TypeA, TypeB>()
		{
			Assert.False(isDBCompatible<TypeA, TypeB>(), "incompatibility test for type {0} failed".format(typeof(TypeA).Name));
		}

		void requireCompatible<TypeA, TypeB>()
		{
			Assert.True(isDBCompatible<TypeA, TypeB>(), "compatible check of {0} failed".format(typeof(TypeA).Name));
		}

		bool isDBCompatible<TypeA, TypeB>()
		{
			using (var storage = createNewDBAndStorageProvider())
			{
				storage.createTableAndIndicesForType<TypeA>();
				return storage.isCompatibleTableExisting<TypeB>();
			}
		}
	}
}
