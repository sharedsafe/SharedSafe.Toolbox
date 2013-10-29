using Toolbox.Testing.LogBased.Detail;

namespace Toolbox.Testing.LogBased
{
	public static class From
	{
		public static ILogMessageFilter Logger(string loggerName)
		{
			return new LogMessageFilter(msg => isLogger(msg.Logger, loggerName));
		}

		public static ILogMessageFilter Logger<TypeT>()
		{
			return new LogMessageFilter(msg => (typeof (TypeT).Name == msg.Logger));
		}

		static bool isLogger(string logger, string match)
		{
			return logger == match || logger.EndsWith("." + match);
		}
	}
}
