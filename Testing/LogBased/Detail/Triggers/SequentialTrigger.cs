using System;

namespace Toolbox.Testing.LogBased.Detail.Triggers
{
	sealed class SequentialTrigger : Trigger
	{
		readonly Trigger _first;
		readonly Trigger _second;

		public SequentialTrigger(Trigger first, Trigger second)
		{
			_first = first;
			_second = second;
		}


		public override IDisposable install(Action triggered)
		{
			IDisposable second = null;

			var first = _first.install(() =>
				{
					second = _second.install(triggered);
				});

			return new DisposeAction(() =>
				{
					if (second != null)
						second.Dispose();

					first.Dispose();
				});
		}
	}
}
