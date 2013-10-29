using System;

namespace Toolbox.Testing.LogBased.Detail
{
	sealed class LogMessageFilter : ILogMessageFilter
	{
		readonly Func<Log.Message, bool> _filter;

		public LogMessageFilter(Func<Log.Message, bool> filter)
		{
			_filter = filter;
		}

		public bool match(Log.Message msg)
		{
			return _filter(msg);
		}
	}
}
