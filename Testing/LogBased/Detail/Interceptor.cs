using System;

namespace Toolbox.Testing.LogBased.Detail
{
	sealed class Interceptor
	{
		readonly ILogMessageFilter _filter;
		readonly Action _action;
		public readonly InterceptorOptions Options;

		public Interceptor(ILogMessageFilter filter, Action action, InterceptorOptions options)
		{
			_filter = filter;
			_action = action;
			Options = options;
		}

		public bool match(Log.Message message)
		{
			return _filter.match(message);
		}

		public void intercept(Log.Message message)
		{
			_action();
		}
	}
}
