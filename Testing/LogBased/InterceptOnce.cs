using System;
using NUnit.Framework.Constraints;
using Toolbox.Testing.LogBased.Detail;

namespace Toolbox.Testing.LogBased
{
	public static class InterceptOnce
	{
		public static void WhenMessage(ILogMessageFilter loggerFilter, Constraint textConstraint, Action action)
		{
			var combinedFilter = Combine.FilterWithConstraint(loggerFilter, textConstraint);

			InterceptManager.install(new Interceptor(combinedFilter, removeLogContext(action), InterceptorOptions.Once));
		}

		static Action removeLogContext(Action action)
		{
			return () =>
				{
					using (Log.pushClearContext())
					{
						action();
					}
				};
		}
	}
}

