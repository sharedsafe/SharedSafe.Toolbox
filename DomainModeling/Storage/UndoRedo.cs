using System;
using System.Collections.Generic;
using Konstruktor;
using RootSE.ORM;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class UndoRedo : IUndoRedo
	{
		readonly Repository<ChangeSet> _changeSets;
		readonly Repository<CompensatingChangeSet> _compensators;
		readonly ChangeSetWriter _writer;
		readonly RedoStack _redoStack;

		public UndoRedo(
			Repository<ChangeSet> changeSets, 
			Repository<CompensatingChangeSet> compensators,
			ChangeSetWriter writer,
			RedoStack redoStack)
		{
			_changeSets = changeSets;
			_compensators = compensators;
			_writer = writer;
			_redoStack = redoStack;
		}

		public bool canUndo()
		{
			return _changeSets.queryCount() != 0;
		}

		public IEnumerable<IDomainEvent> undo()
		{
			ChangeSet cs = null;
			IEnumerable<IDomainEvent> denormalizedEvents = null;

			_changeSets.transact(() =>
				{
					var count = _changeSets.queryCount();
					if (count == 0)
						throw new Exception("Can't undo, no changesets");

					cs = _changeSets.get(count);
					var id = cs.Guid;
					var denormalizedCompensator = _compensators.get(id);

					// remove from changesets and compensating changesets
					_changeSets.delete(count);
					_compensators.delete(id);

					denormalizedEvents = denormalizedCompensator.domainEventsOf();

					// run the compensators against the views (they have been written denormalized)
					_writer.applyToModelAndViews(denormalizedEvents);
				});

			_redoStack.push(cs);

			return denormalizedEvents;
		}

		public bool canRedo()
		{
			return _redoStack.canRedo();
		}

		public IEnumerable<IDomainEvent> redo()
		{
			if (!_redoStack.canRedo())
				throw new Exception("Can't redo, no changesets");

			var intentional = _redoStack.pop();
			try
			{
				return _writer.storeAndUpdateViews(intentional.domainEventsOf());
			}
			catch (Exception)
			{
				// if something happens in the transaction, we are out of sync with the redo stack, so clear it.
				_redoStack.clear();
				throw;
			}
		}
	}
}
