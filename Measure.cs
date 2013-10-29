using System;
using System.Diagnostics;

namespace Toolbox
{
	public static class Measure
	{
		// Time the using enclosed method and call the elapsed action with the time elapsed.

		public static IDisposable time(Action<TimeSpan> elapsed)
		{
			var sw = Stopwatch.StartNew();
			return new DisposeAction(() => elapsed(sw.Elapsed));
		}

		public static TimeSpan time(Action action)
		{
			var sw = Stopwatch.StartNew();
			action();
			return sw.Elapsed;
		}
	}
}
