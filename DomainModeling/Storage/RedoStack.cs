using System.Collections.Generic;

namespace DomainModeling.Storage
{
	sealed class RedoStack
	{
		readonly Stack<ChangeSet>  _redoStack = new Stack<ChangeSet>();

		public void push(ChangeSet intentional)
		{
			_redoStack.Push(intentional);
		}

		public bool canRedo()
		{
			return _redoStack.Count != 0;
		}

		public ChangeSet pop()
		{
			return _redoStack.Pop();
		}

		public void clear()
		{
			_redoStack.Clear();
		}
	}
}
