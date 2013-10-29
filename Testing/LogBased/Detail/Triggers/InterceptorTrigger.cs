using System;

namespace Toolbox.Testing.LogBased.Detail.Triggers
{
	sealed class InterceptorTrigger : Trigger
	{
		readonly ILogMessageFilter _filter;

		public InterceptorTrigger(ILogMessageFilter filter)
		{
			_filter = filter;
		}

		public override IDisposable install(Action triggered)
		{
			var interceptor = new Interceptor(_filter, triggered, InterceptorOptions.Once);
			return InterceptManager.install(interceptor);
		}
	}
}
