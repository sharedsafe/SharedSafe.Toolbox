using System;
using System.Diagnostics;

namespace Toolbox.Shell
{
	public static class Browser
	{
		public static void showURL(string url)
		{
			try
			{
				var process = Process.Start(url);
				if (process != null)
					process.Dispose();
			}
			catch (Exception e)
			{
				Log.E("Failed to view URL: " + url);
				Log.E(e);
			}
		}
	}
}
