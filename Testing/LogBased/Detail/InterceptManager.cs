using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Testing.LogBased.Detail
{
	static class InterceptManager
	{
		static readonly object _syncRoot = new object();
		static readonly List<Interceptor> _interceptors = new List<Interceptor>();

		static InterceptManager()
		{
			// this clears other hooks, which it shouldn't
			Log.PostLoggingHook_ = hook;
		}


		public static IDisposable install(Interceptor interceptor)
		{
			lock (_syncRoot)
				_interceptors.Add(interceptor);

			return new DisposeAction(() =>
				{
					lock (_syncRoot)
						_interceptors.Remove(interceptor);
				});
		}

		static void hook(Log.Message message)
		{
			// Debug.WriteLine(">>! logger: " + message.Logger);
			lock (_syncRoot)
			{
				foreach (var interceptor in _interceptors.Where(interceptor => interceptor.match(message)))
				{
					if ((interceptor.Options & InterceptorOptions.Once) != 0)
						_interceptors.Remove(interceptor);

					interceptor.intercept(message);
					return;
				}
			}
		}
	}
}
