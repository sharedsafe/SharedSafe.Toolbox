using Konstruktor;

namespace DomainModeling.Detail
{
	[DefaultImplementation]
	sealed class DomainEvolutionService : IDomainEvolutionService
	{
		readonly VersionTable _versionTable;

		public DomainEvolutionService(VersionTable versionTable)
		{
			_versionTable = versionTable;
		}

		const string DomainModelAndViewsKey = "DomainModelAndViews";

		public bool shouldRebuildDomainTables(int version)
		{
			var current = _versionTable.queryVersion(DomainModelAndViewsKey);
			return current != version;
		}

		public void confirmCurrentSchema(int version)
		{
			_versionTable.storeVersion(DomainModelAndViewsKey, version);
		}
	}
}