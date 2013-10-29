/**
	Serializable progress reports.

	Progress reports can be tracked by anyone pushing an Action<ProgressReport> context.
**/

#if DEBUG
// #define PROGRESSREPORTING_SLOWDOWN
#endif

using System;
using System.Diagnostics;

#if PROGRESSREPORTING_SLOWDOWN
using System.Threading;
#endif

namespace Toolbox
{
	public static class ProgressReporting
	{
		#region Span a reporting scope

		public static IDisposable pushScope(Action<ProgressReport> action)
		{
			return Context<Action<ProgressReport>>.push(action);
		}

		#endregion

		#region Task Model (tracks exceptions)

		public static void runTask(string description, Action task)
		{
			using (beginTask(description))
			{
				tryRun(task);
			}
		}

		public static void runTask(string description, int subTasks, Action task)
		{
			using (beginTask(description, subTasks))
			{
				tryRun(task);
			}
		}

		public static void runPercentageTask(string description, Action task)
		{
			using (beginPercentageTask(description))
			{
				tryRun(task);
			}
		}

		#endregion

		/// Begin the progress of a tasks, the error is cleared.

		#region Step tasks

		// Note: Zero step tasks are supported.

		/// Single step task.

		static IDisposable beginTask(string description)
		{
			return beginInternal(new ProgressTaskBegin { Description = description });
		}

		static IDisposable beginTask(string description, int subTasks)
		{
			Debug.Assert(subTasks >= 0);
			return beginInternal(new ProgressTaskBeginSteps { Description = description, SubTasks = (uint)subTasks });
		}

		#endregion

		#region Percentage tasks

		static IDisposable beginPercentageTask(string description)
		{
			return beginInternal(new ProgressTaskBeginPercentage { Description = description });
		}

		#endregion

		static IDisposable beginInternal(ProgressTaskBegin taskBegin)
		{
			Error = null;

			if (ReportingEnabled)
				// (nice: tasks get realized only if someone actually listens to progress reports)
				reportInternal(taskBegin);

			return new DisposeAction(() =>
			{
				if (ReportingEnabled)
					reportInternal(new ProgressTaskEnd { Error_ = Error });
			});
		}

		public static void percentage(uint percentage)
		{
			if (ReportingEnabled)
				reportInternal(new ProgressPercentage { Percentage = percentage });
		}


		static bool ReportingEnabled
		{
			get { return Context<Action<ProgressReport>>.Available; }
		}

		public static bool report(ProgressReport report)
		{
			if (!ReportingEnabled)
				return false;
			reportInternal(report);
			return true;
		}

		static void reportInternal(ProgressReport report)
		{
			Debug.Assert(ReportingEnabled);
			Context<Action<ProgressReport>>.Current(report);
#if PROGRESSREPORTING_SLOWDOWN
			// don't stall on percentage reports :)
			if (!(report is ProgressPercentage))
				Thread.Sleep(500);
#endif
		}


		#region Error Handling

		/// Run progress parts and report exceptions (and _rethrows_ them), 
		/// returns true on success and false if an error will be
		/// send to the client.
		
		public static void tryRun(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				error(e);
				/* re */ throw;
			}
		}

		/// Set a task progress error, that is transferred to the cleint when begin().Dispose is called.

		public static void error(string description)
		{
			Error = description;
		}

		/// Report a cached exception.

		public static void error(Exception e)
		{
			Error = e.Message;
		}

		#endregion

		[ThreadStatic]
		static string Error;
	}

	#region Progress Reports (Nicely Serializable)

	public abstract class ProgressReport
	{
	};

	/// A task with an unknown number of substeps.

	public class ProgressTaskBegin : ProgressReport
	{
		public string Description { get; set; }
	
		public override string ToString()
		{
			return Description;
		}
	};

	/// A task with a known number of substeps.

	public sealed class ProgressTaskBeginSteps : ProgressTaskBegin
	{
		public uint SubTasks { get; set; }

		public override string ToString()
		{
			return " (" + SubTasks + " steps)";
		}
	}

	/// A task with no substeps, but percentage indication.

	public sealed class ProgressTaskBeginPercentage : ProgressTaskBegin
	{
	}

	public sealed class ProgressPercentage : ProgressReport
	{
		public ProgressPercentage()
		{
			Info = string.Empty;
		}

		public uint Percentage { get; set; }
		public string Info { get; set; }

		public override string ToString()
		{
			return Percentage + "% " + Info;
		}
	};

	public sealed class ProgressTaskEnd : ProgressReport
	{
		// task error, if any, null: no error
		public string Error_ { get; set; }

		public override string ToString()
		{
			return "Task ended: " + Error_ ?? "No Error";
		}
	}

	#endregion
}
