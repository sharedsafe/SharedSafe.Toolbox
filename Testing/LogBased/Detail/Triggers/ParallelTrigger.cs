using System;

namespace Toolbox.Testing.LogBased.Detail.Triggers
{
	sealed class ParallelTrigger : Trigger
	{
		readonly Trigger _one;
		readonly Trigger _two;

		public ParallelTrigger(Trigger one, Trigger two)
		{
			_one = one;
			_two = two;
		}

		public override IDisposable install(Action triggered)
		{
			bool tOne = false;
			bool tTwo = false;

			var one = _one.install(() =>
				{
					tOne = true;
					if (tTwo)
						triggered();
				});

			var two = _two.install(() =>
				{
					tTwo = true;
					if (tOne)
						triggered();
				});

			return new DisposeAction(() =>
				{
					two.Dispose();
					one.Dispose();
				});
		}
	}
}
