using System;
using System.Diagnostics;
using Toolbox.Logging;

namespace Toolbox
{
	public interface ILogContext
	{
		void T(string str);
		void D(string str);
		void I(string str);

		void W(string str);
		void E(string str);
		void E(Exception e);
		Exception error(string description, params object[] capturedData);
	}

	public static class ILogContextExtensions
	{
		// nice forwarder, useful when ILogContext is named Log
		public static IDisposable pushContext(this ILogContext c, string context)
		{
			return Log.pushContext(context);
		}
	}

	public static class Log
	{
		/// The detection prefix may be used for discrimination of the log

		const string DetectionPrefix = ">>!";

		// note: adjust SeverityPrefixes and NLog LogLevels arrays when changing the Severity enum.
		public enum Severity
		{
			Trace,
			Debug,
			Info,
			Warning,
			Error
		}

		public struct Message
		{
			public readonly Severity Severity;
			public readonly string Logger;
			public readonly string Text;
			public readonly object Instance_;

			public Message(Severity severity, string logger, string text, object instance)
			{
				Severity = severity;
				Logger = logger;
				Text = text;
				Instance_ = instance;
			}
		}

		static readonly string[] SeverityPrefixes = new[]
		{
			DetectionPrefix + " T ",
			DetectionPrefix + " D ",
			DetectionPrefix + " I ",
			DetectionPrefix + " W ",
			DetectionPrefix + " E "
		};
	
		[Conditional("TRACE"), Conditional("DEBUG")]
		public static void T(string output)
		{
			log(Severity.Trace, output);
		}

		[Conditional("DEBUG")]
		public static void D(string output)
		{
			log(Severity.Debug, output);
		}

		[Conditional("DEBUG")]
		public static void I(string output)
		{
			log(Severity.Info, output);
		}

		public static void W(string output)
		{
			log(Severity.Warning, output);
		}

		public static void W(Exception e)
		{
			E(e.Message);
#if DEBUG
			D(e.ToString());
#endif
		}

		public static void E(string output)
		{
			log(Severity.Error, output);
		}

		public static void E(Exception e)
		{
			E(e.Message);
#if DEBUG
			D(e.ToString());
#endif
		}

		#region Context Logic

		internal static string ContextPrefix
		{
			get
			{
				var ctx = Context<ContextScope>.CurrentOrDefault;
				return ctx.Context ?? string.Empty;
			}
		}

		/// Put in a context for logging. The context string will be prepended to
		/// each log entry.

		public static IDisposable pushContext(string context)
		{
			var escapedContext = escapeContext(context);

			if (!Context<ContextScope>.Available)
				return pushPrefix(escapedContext);
			var parent = Context<ContextScope>.Current;
			return Context.push(new ContextScope(context, combineContext(parent.Context, escapedContext)));
		}


		public static IDisposable pushClearContext()
		{
			if (!Context<ContextScope>.Available)
				return new DisposeAction(() => { });

			return Context.push(new ContextScope("", ""));
		}

		internal static string escapeContext(string context)
		{
			return context.Replace('.', '_');
		}

		// made non-public because it does not seem to get used.

		static IDisposable pushPrefix(string prefix)
		{
			return Context.push(new ContextScope(prefix, prefix));
		}

#if false
		// not used?

		/// Define a sub-context for logging. The txt string will be appended to
		/// the current context's content string.

		public static IDisposable pushSubContext(string txt)
		{
			var current = CurrentPrefix;
			return current == null ? pushContext(txt) : pushContext(current + txt);
		}
#endif

		/// Returns null if there is no current context set.

		public static string CurrentPrefix
		{
			get
			{
				return Context<ContextScope>.Available ? Context<ContextScope>.Current.Prefix : null;
			}
		}

		struct ContextScope
		{
			public ContextScope(string prefix, string context)
			{
				Prefix = prefix;
				Context = context;
			}

			public readonly string Prefix;
			public readonly string Context;
		};

		#endregion


		#region Object extensions

		[Conditional("TRACE")]
		public static void T(this object obj, string str)
		{
			logWithInstance(Severity.Trace, obj, str);
		}

		[Conditional("DEBUG")]
		public static void D(this object obj, string str)
		{
			logWithInstance(Severity.Debug, obj, str);
		}

		public static IDisposable DScope(this object obj, string str)
		{
#if !DEBUG
			return DisposableDummy.Instance;
#else

			D("++ " + objectize(obj, str));
			var ctx = pushPrefix("   ");

			return new DisposeAction(() =>
				{
					ctx.Dispose();
					D("-- " + objectize(obj, str));
				});
#endif
		}

		[Conditional("DEBUG")]
		public static void I(this object obj, string str)
		{
			logWithInstance(Severity.Info, obj, str);
		}

		/**
			We may propagate Exception messages down to level info.
		**/

		[Conditional("DEBUG")]
		public static void I(this object obj, Exception e)
		{
			obj.I(e.Message);
			obj.D(e.ToString());
		}

		public static void W(this object obj, string str)
		{
			logWithInstance(Severity.Warning, obj, str);
		}

		public static void E(this object obj, string str)
		{
			logWithInstance(Severity.Error, obj, str);
		}

		public static void W(this object obj, Exception e)
		{
			obj.W(e.Message);
			obj.D(e.ToString());
		}

		public static void E(this object obj, Exception e)
		{
			obj.E(e.Message);
			obj.D(e.ToString());
		}

		#region Canonical Log Methods


		static void logWithInstance(Severity severity, object obj, string message)
		{
			using (pushContext(presentObject(obj)))
			{
				log(severity, message);
			}
		}


		internal static void log(Severity severity, string output)
		{
			logWithCurrentContext(severity, output);

			var postLoggingHook = PostLoggingHook_;
			if (postLoggingHook != null)
			{
				var msg = new Message(severity, ContextPrefix, output, null);
				postLoggingHook(msg);
			}
		}
		#endregion

		static void logWithCurrentContext(Severity severity, string output)
		{
			var context = ContextPrefix;
#if USE_NLOG
			NLogTarget.logWithContext(severity, context, output);
#elif USE_SMARTINSPECT
			SmartInspectTarget.logWithContext(severity, context, output);
#else
			WriteLines(SeverityPrefixes[(int)severity] + context, output.Split('\n'));
#endif

		}

		#region Hooks (for Log Based Testing)

		static volatile Action<Message> _postLoggingHook_;

		public static Action<Message> PostLoggingHook_
		{
			get { return _postLoggingHook_; }
			set { _postLoggingHook_ = value; }
		}
		
		#endregion

		/// Returns an exception instance that contains the context of the current object (usage: this.error())

		public static Exception error(this object obj, string description, params object[] capturedData)
		{
			var e = new InternalError(objectize(obj, description), capturedData);
			obj.W("throwing " + e.Message);
			return e;
		}

		public static Exception error(this object obj, Exception inner, string description)
		{
			var e = new InternalError(inner, objectize(obj, description));
			obj.W("throwing " + e.Message);
			return e;
		}

		#endregion


		#region Thread Local Output Context

		public static IDisposable pushOutputContext(Action<string, string[]> writeLine)
		{
			return Context<LogOutputContext>.push(new LogOutputContext(writeLine));
		}

		struct LogOutputContext
		{
			public LogOutputContext(Action<string, string[]> writeLines)
			{
				WriteLines = writeLines;
			}

			public readonly Action<string, string[]> WriteLines;
		}

		public static Action<string, string[]> WriteLines
		{
			get
			{
				var loc = Context<LogOutputContext>.CurrentOrDefault;
				
#if IOS
				return loc.WriteLines ?? GlobalWriteLines ?? consoleWriteLines;
#else
				return loc.WriteLines ?? GlobalWriteLines ?? debugWriteLines;
#endif
			}
		}

		public static Action<string, string[]> GlobalWriteLines { get; set; }


		static void debugWriteLines(string prefix, string[] lines)
		{
			if (prefix != string.Empty)
				prefix += ": ";

			foreach (var line in lines)
				Debug.WriteLine(prefix + line);
		}

		static void consoleWriteLines(string prefix, string[] lines)
		{
			if (prefix != string.Empty)
				prefix += ": ";

			foreach (var line in lines)
				Console.WriteLine(prefix + line);
		}


		#endregion
#if !DEBUG
		sealed class DisposableDummy : IDisposable
		{
			public void Dispose()
			{}

			public static readonly DisposableDummy Instance = new DisposableDummy();
		}
#endif

		// sad: Static types can not be used as type arguments.
		public static ILogContext createContext(Type t)
		{
			return new LogContext(t.Name);
		}


		static string objectize(object obj, string str)
		{
			if (obj == null)
				return "null";

			// strip if ToString() returns the full type name.
			var name = obj.ToString();
			if (name == obj.GetType().FullName)
				name = obj.GetType().Name;

			return name + ": " + str;
		}

		static string presentObject(object instance)
		{
			if (instance == null)
				return "null";

			var name = instance.ToString();
			if (name == instance.GetType().FullName)
				name = instance.GetType().Name;

			return name;
		}

		static string combineContext(string parent, string context)
		{
			if (parent == string.Empty)
				return context;

			return parent + "." + context;
		}
	}
}
