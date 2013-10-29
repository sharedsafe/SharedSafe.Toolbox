using System;

namespace Toolbox
{
	public static class LogWithDate
	{
		public static IDisposable use()
		{
			var previousWriter = Log.WriteLines;
			return Log.pushOutputContext((p, lines) => writeWithTimestamp(previousWriter, p, lines));
		}

		static void writeWithTimestamp(Action<string, string[]> prev, string prefix, string[] lines)
		{
			var now = DateTime.Now;
			// ISO8601 (before the .) (we do not use the T separator)
			var nowStr = now.ToString("yyyyMMddTHHmmss.fff");
			var newPrefix = nowStr + " " + prefix;
			prev(newPrefix, lines);
		}
	}
}
