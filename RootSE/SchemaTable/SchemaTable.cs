#if false
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Schema;

namespace RootSE.SchemaTable
{
	public sealed class SchemaTable
	{
		internal readonly Dictionary<string, JsonSchema> _schemas;

		public SchemaTable(Dictionary<string, JsonSchema> schemas)
		{
			_schemas = schemas;
		}

		public bool isCompatibleWith(SchemaTable right)
		{
			return equals(_schemas, right._schemas);
		}

		public static SchemaTable createFromTypes(IEnumerable<Type> types)
		{
			var generator = new JsonSchemaGenerator();
			var dict = types.ToDictionary(t => t.Name, generator.Generate);
			return new SchemaTable(dict);
		}

		static bool equals(IDictionary<string, JsonSchema> left, IDictionary<string, JsonSchema> right)
		{
			if (left.Count != right.Count)
				return false;

			foreach (var l in left)
			{
				JsonSchema r;
				if (!right.TryGetValue(l.Key, out r))
					return false;

				if (!JsonSchemaEqualityComparer.equals(l.Value, r))
					return false;
			}

			return true;
		}
	}
}
#endif
