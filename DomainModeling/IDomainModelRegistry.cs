using DomainModeling.Meta;

namespace DomainModeling
{
	public interface IDomainModelRegistry
	{
		MetaType byName(string name);
		MetaType byDestructiveEvent(IDestructiveDomainEvent destructiveEvent);
	}
}