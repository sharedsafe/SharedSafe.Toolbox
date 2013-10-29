#if FALSE
using NUnit.Framework;
using Newtonsoft.Json.Schema;
using RootSE.SchemaTable;
using Toolbox;

namespace RootSETests
{
	[TestFixture]
	class SchemaTable
	{
		sealed class Aggregate
		{
			public Nested Nested;
		}

		sealed class Nested
		{
			public uint NestedValue;
		}


		[Test]
		public void expectedNestedToBeIncludedInSchema()
		{
			var generator = new JsonSchemaGenerator();
			var schema = generator.Generate(typeof (Aggregate));
			this.D(schema.ToString());

			Assert.That(schema.ToString(), Contains.Substring("\"type\": \"integer\""));
		}

		sealed class Ordered1
		{
			public uint b;
			public uint a;
		}

		sealed class Ordered2
		{
			public uint a;
			public uint b;
		}

		[Test]
		public void expectEqualOnDifferentPropertyOrdering()
		{
			var generator = new JsonSchemaGenerator();
			var schema1 = generator.Generate(typeof(Ordered1));
			var schema2 = generator.Generate(typeof(Ordered2));
			Assert.True(JsonSchemaEqualityComparer.@equals(schema1, schema2));
		}

		sealed class Root1
		{
			public Ordered1 Value;
		}

		sealed class Root2
		{
			public Ordered2 Value;
		}

		[Test]
		public void expectEqualOnDifferentNestedPropertyOrdering()
		{
			var generator = new JsonSchemaGenerator();
			var schema1 = generator.Generate(typeof(Root1));
			var schema2 = generator.Generate(typeof(Root2));
			Assert.True(JsonSchemaEqualityComparer.@equals(schema1, schema2));
		}
	}
}
#endif
