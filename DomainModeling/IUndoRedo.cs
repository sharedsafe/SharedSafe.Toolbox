using System.Collections.Generic;

namespace DomainModeling
{
	public interface IUndoRedo
	{
		bool canUndo();
		IEnumerable<IDomainEvent> undo();
		
		bool canRedo();
		IEnumerable<IDomainEvent> redo();
	}
}