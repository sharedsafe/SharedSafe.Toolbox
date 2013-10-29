using System;

namespace Toolbox.Timing
{
	public sealed class CancelableSingleRunAction : IDisposable
	{
		readonly Action _action;
		volatile bool _allowedToRun;

		public CancelableSingleRunAction(Action action)
		{
			_action = action;
			_allowedToRun = true;
		}

		public void run()
		{
			if (_allowedToRun)
			{
				_action();
				_allowedToRun = false;
			}
		}

		public void Dispose()
		{
			_allowedToRun = false;
		}

		public static Pair<Action, IDisposable> create(Action action)
		{
			var cancellable = new CancelableSingleRunAction(action);
			return Pair.make<Action, IDisposable>(cancellable.run, cancellable);
		}
	}
}
