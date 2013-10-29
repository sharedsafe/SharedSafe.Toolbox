using System;
using System.Threading;

namespace Toolbox.Timing
{
	public static class Retry
	{
		public static ResT untilNotNullOrTimeout<ResT>(Func<ResT> function, uint waitBetweenTriesMS, uint maxRetries)
			where ResT: class
		{
			return core(waitBetweenTriesMS, maxRetries, function);
		}

		public static ResT untilNotNullForever<ResT>(Func<ResT> action, uint waitBetweenRetriesMS)
			where ResT: class
		{
			return core(waitBetweenRetriesMS, null, action);
		}


		static ResT core<ResT>(uint waitBetweenTriesMS, uint? maxRetriesMS, Func<ResT> action)
			where ResT: class
		{
			var currentRetry = 0;

			for (; ; )
			{
				var r = action();
				if (r != null)
					return r;

				if (maxRetriesMS != null && maxRetriesMS.Value == currentRetry)
					return null;

				if (waitBetweenTriesMS > 0)
					Thread.Sleep(waitBetweenTriesMS.signed());
				++currentRetry;
			}
		}
	}
}
