#if false
using System.Linq;
using Newtonsoft.Json.Schema;
using RootSE.ORM;
using RootSE.Provider;

namespace RootSE.SchemaTable
{
	public sealed class SchemaTableRepository
	{
		readonly Repository<SchemaInfo> _repository;

		public SchemaTableRepository(Repository<SchemaInfo> repository)
		{
			_repository = repository;
		}

		public SchemaTable load()
		{
			var dict = _repository.queryAll().ToDictionary(
				si => si.TypeName,
				si => JsonSchema.Parse(si.JsonSchema));

			return new SchemaTable(dict);
		}

		public void store(SchemaTable table)
		{
			using (var transaction = _repository.beginTransaction())
			{
				_repository.delete(Criteria.All);

				foreach (var t in table._schemas)
				{
					var typeName = t.Key;
					var schema = t.Value;

					var info = new SchemaInfo()
					{
						TypeName = typeName,
						JsonSchema = schema.ToString()
					};

					_repository.insert(info);
				}

				transaction.commit();
			}
		}
	}
}
#endif
