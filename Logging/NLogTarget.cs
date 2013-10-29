#if USE_NLOG
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Toolbox.Logging
{
	static class NLogTarget
	{
		public static void logWithContext(Log.Severity severity, string context, string output)
		{
			var logger = LogManager.GetLogger(context);
			logger.Log(Severity2LogLevel[(int)severity], output);
		}

		static readonly LogLevel[] Severity2LogLevel = new[]
		{
			LogLevel.Trace,
			LogLevel.Debug,
			LogLevel.Info,
			LogLevel.Warn,
			LogLevel.Error
		};

	
		static NLogTarget()
		{
			LogManager.ThrowExceptions = true;
		
			var config = new LoggingConfiguration();


			// should be named log4j
			var chainsawTarget = new ChainsawTarget();
			chainsawTarget.Name = "cs";
			chainsawTarget.Address = "udp4://localhost:7071";
			// chainsawTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";


			var target = new OutputDebugStringTarget();
			target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

			config.AddTarget("ds", target);
			config.AddTarget("cs", chainsawTarget);

			var rule = new LoggingRule("*", target);
			// wtf?
			foreach (var l in Severity2LogLevel)
				rule.EnableLoggingForLevel(l);

			var chainSawRule = new LoggingRule("*", chainsawTarget);
			// wtf?
			foreach (var l in Severity2LogLevel)
				chainSawRule.EnableLoggingForLevel(l);

			config.LoggingRules.Add(rule);
			config.LoggingRules.Add(chainSawRule);
			LogManager.Configuration = config;
		}
	}
}
#endif
