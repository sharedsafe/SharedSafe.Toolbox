using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Toolbox.Environment
{
	public enum RunOnce
	{
		PerUser,
		/// Warn: systemwide needs special access rights!
		SystemWide
	}

	public static class ProcessContext
	{
		public static bool runOncePerUser(string globalKey, bool showWindowOfAlreadyRunningProcess, Action run)
		{
			var disp = tryRunOncePerUser(globalKey, showWindowOfAlreadyRunningProcess);
			if (disp == null)
			{
				Log.I(globalKey + ": already running.");
				return false;
			}

			using (disp)
			{
				run();
				return true;
			}
		}

		public static IDisposable tryRunOncePerUser(string globalKey, bool showWindowOfAlreadyRunningProcess, int timeoutMS = 1000)
		{
			return tryRunOnce(RunOnce.PerUser, globalKey, showWindowOfAlreadyRunningProcess, timeoutMS);
		}

		public static IDisposable tryRunOnce(RunOnce runOnce, string globalKey, bool showWindowOfAlreadyRunningProcess, int timeoutMS)
		{
			var prefix = runOnce == RunOnce.SystemWide ? "Global\\" : "";
			var mutex = new Mutex(false, prefix + globalKey + ".RunOnceMutex");

			if (mutex.WaitOne(timeoutMS))
				return new DisposeAction(() =>
				{
					mutex.ReleaseMutex();
					mutex.Close();
				});

			if (showWindowOfAlreadyRunningProcess)
				ProcessContext.showWindowOfAlreadyRunningProcess();

			return null;
		}

		public static bool isRunningForThisUser(string globalKey)
		{
			var disp_ = tryRunOncePerUser(globalKey, false);
			if (disp_ == null)
				return true;
			disp_.Dispose();
			return false;
		}

		static void showWindowOfAlreadyRunningProcess()
		{
			var myId = Process.GetCurrentProcess().Id;

			var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			foreach (var p in processes)
			{
				if (p.Id == myId)
					continue;

				var mainWindow = p.MainWindowHandle;
				if (mainWindow == IntPtr.Zero)
					continue;
				ShowWindow(mainWindow, 1);
				SetForegroundWindow(mainWindow);
				// the first is fine
				return;
			}
		}

		[DllImport("User32.dll")]
		public static extern Int32 SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	}
}
