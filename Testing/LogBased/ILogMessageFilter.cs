using Toolbox.Testing.LogBased.Detail;

namespace Toolbox.Testing.LogBased
{
	public interface ILogMessageFilter
	{
		bool match(Log.Message message);
	}

	public static class LogMessageFilterExtensions
	{
		public static ILogMessageFilter chain(this ILogMessageFilter filter, ILogMessageFilter next)
		{
			return new LogMessageFilter(msg => filter.match(msg) && next.match(msg));
		}
	}
}
