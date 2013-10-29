namespace DomainModeling
{
	public interface IBootstrapFromDomainObject<in DomainObjectT>
		where DomainObjectT:IDomainObject
	{
		void bootstrap(DomainObjectT model);
	}
}
