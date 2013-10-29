using System.Diagnostics;
using System.Linq;
using RootSE.ORM;
using RootSE.Provider;

namespace DomainModeling.Detail
{
	sealed class VersionTable
	{
		readonly Repository<VersionRecord> _repository;

		public VersionTable(Repository<VersionRecord> repository)
		{
			_repository = repository;
		}

		public static Criteria key(string key)
		{
			return Term.column((VersionRecord l) => l.Key).equals(key);
		}

		public const int UnknownVersion = 0;

		public int queryVersion(string key)
		{
			var all = _repository.query(VersionTable.key(key)).ToArray();
			return !all.Any() ? 0 : all.Single().Version;
		}

		public void storeVersion(string key, int version)
		{
			Debug.Assert(version != 0);
			var oldVersion = queryVersion(key);
			if (oldVersion == 0)
				_repository.insert(new VersionRecord {Key = key, Version = version});
			else
				_repository.update(new VersionRecord {Key = key, Version = version});
		}
	}
}
