namespace DomainModeling
{
	public interface IHandleEvent<in EventT>
		where EventT: IDomainEvent
	{
		void handle(EventT ev);
	}
}