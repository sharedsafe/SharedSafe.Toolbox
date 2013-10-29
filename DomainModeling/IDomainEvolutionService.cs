namespace DomainModeling
{
	public interface IDomainEvolutionService
	{
		bool shouldRebuildDomainTables(int version);
		void confirmCurrentSchema(int version);
	}
}
