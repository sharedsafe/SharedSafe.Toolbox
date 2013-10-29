/**
	The extensions here provide progress reports over an IPC end point.

	They work closely together with the Toolbox.Progress methods.
**/

using System;
using System.Threading;

namespace Toolbox.IPC
{
	public static class ProgressReportingExtensions
	{
		#region Reporting Progress to IPC endpoints

		/**
			Run the assigned task in a progress reporting context: 
		
			This sends out all progress reports over the object end point. 
		

			In case of an exception in task:

				If rethrowTaskExceptions is true, 
					this method (re) throws the exception from the task, and returns true in case of success.
			
				If rethrowTaskExceptions is false, 
					this method returns false in case of an exception in task.

				In any case, the error is reported using the progress reports.

			In case of an exception in the progress report delivery:
				The exception is immediately thrown to the caller.
		**/

		/// Default does not throw.

		public static bool reportProgress(this IObjectEndPoint ep, Action task)
		{
			return ep.reportProgress(task, false);
		}

		public static bool reportProgress(this IObjectEndPoint ep, Action task, bool rethrowTaskException)
		{
			Action<ProgressReport> reportAction = pr =>
			{
				// if we got an IPC error, we should tunnel it through
				try
				{
					ep.send(pr);
				}
				catch (Exception e)
				{
					throw new ProgressReportIOException(e);
				}
			};


			using (Context.push(reportAction))
			{
				try
				{
					task();
					return true;
				}
				catch (ProgressReportIOException e)
				{
					throw e.InnerException;
				}
				catch (Exception e)
				{
					// note:
					// clients need to use Progress.tryRun() to catch errors and report them
					ep.E(e.Message);
					ep.D(e.ToString());

					if (rethrowTaskException)
						throw;

					return false;
				}
			}
		}

		sealed class ProgressReportIOException : Exception
		{
			public ProgressReportIOException(Exception inner)
				: base("Progress report IO exception", inner)
			{
			}
		}
		#endregion

		#region Dispatching Progress reports from IPC endpoints

		/**
			We need to dispatch them separately, because we actually can only find out by 
			logging the number of BeginTask and EndTask reports when the communication shall end.

			Right now, by definition, the protocol needs to send properly nested ProgressTaskBegin / End Pairs,
			so the last ProgressTaskEnd defines the final error result and is then returned.
		**/

		public static string dispatchProgress(this IObjectEndPoint ep, Action<ProgressReport> report)
		{
			return dispatchProgress(ep, report, null);
		}

		public static string dispatchProgress(this IObjectEndPoint ep, Action<ProgressReport> report, WaitHandle interrupt_)
		{
			uint tasksRunning = 0;
			ProgressReport r;

			do
			{
				r = ep.tryReceive<ProgressReport>(interrupt_);
				if (r == null)
					return ep.EOF ? "Process disconnected unexpectedly." : null;

				if (r is ProgressTaskBegin)
					++tasksRunning;
				else if (r is ProgressTaskEnd)
					--tasksRunning;

				report(r);
			} while (tasksRunning != 0);

			var end = r as ProgressTaskEnd;
			return end != null ? end.Error_ : null;
		}

		#endregion
	}
}
