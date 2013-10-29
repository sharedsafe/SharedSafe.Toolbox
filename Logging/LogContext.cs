using System;

namespace Toolbox.Logging
{
	sealed class LogContext : ILogContext
	{
		readonly string _name;

		public LogContext(string name)
		{
			_name = name;
		}

		public void T(string str)
		{
			log(Log.Severity.Trace, str);
		}

		public void D(string str)
		{
			log(Log.Severity.Debug, str);
		}

		public void I(string str)
		{
			log(Log.Severity.Info, str);
		}

		public void W(string str)
		{
			log(Log.Severity.Warning, str);
		}

		public void E(string str)
		{
			log(Log.Severity.Error, str);
		}

		void log(Log.Severity severity, string str)
		{
			using (Log.pushContext(_name))
			{
				Log.log(severity, str);
			}
		}
	
		public void E(Exception e)
		{
			E(e.Message);
			D(e.ToString());
		}

		public Exception error(string description, params object[] capturedData)
		{
			var e = new InternalError(objectize(description), capturedData);
			W("throwing " + e.Message);
			return e;
		}

		string objectize(string str)
		{
			return _name + ": " + str;
		}
	}
}
