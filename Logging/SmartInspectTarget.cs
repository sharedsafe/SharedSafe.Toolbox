#if USE_SMARTINSPECT

using System;
using System.Collections.Generic;
using Gurock.SmartInspect;

namespace Toolbox.Logging
{
	internal class SmartInspectTarget
	{
		public static void logWithContext(Log.Severity severity, string context, string output)
		{
			var session = resolveSession(context);

			switch (severity)
			{
				case Log.Severity.Trace:
					session.LogVerbose(output);
					break;
				case Log.Severity.Debug:
					session.LogDebug(output);
					break;
				case Log.Severity.Info:
					session.LogMessage(output);
					break;
				case Log.Severity.Warning:
					session.LogWarning(output);
					break;
				case Log.Severity.Error:
					session.LogError(output);
					break;
				default:
					throw new ArgumentOutOfRangeException("severity");
			}
		}

		static readonly object _syncRoot = new object();

		static readonly Dictionary<Session, List<Session>> _sessions = new Dictionary<Session, List<Session>>();

		static Session resolveSession(string context)
		{
			lock (_syncRoot)
			{
				var session = SiAuto.Si.GetSession(context);
				if (session == null)
				{
					session = SiAuto.Si.AddSession(context);
				}
				return session;
			}
		}

		static SmartInspectTarget()
		{
			SiAuto.Si.Enabled = true;
		}
	}
}

#endif
