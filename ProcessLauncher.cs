using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Toolbox
{
	/**
		Launches processes.

		Either locally where only an executable name is given. 
		Then the process is run in the same directory and the working directories are set appropriately.

		Or globally, given a path from which also the working directory is extracted.
	**/

	public static class ProcessLauncher
	{
		public static void launch(string absoluteOrRelativePath, string arguments_ = null)
		{
			var info = createStartInfoFor(absoluteOrRelativePath, arguments_);
			var process = Process.Start(info);
			if (process == null)
				throw new Exception("Failed to start process {0} with arguments {1}".format(absoluteOrRelativePath, arguments_));
			process.Dispose();
		}

		// if a relative filename is given, the filename is treated relative to the Toolbox.dll's directory!

		public static ProcessStartInfo createStartInfoFor(string absoluteOrRelativePath, string arguments_ = null)
		{
			var rootedPath = ensurePathRooted(absoluteOrRelativePath);
			var dir = Path.GetDirectoryName(rootedPath);
			Debug.Assert(dir != null);

			var info = new ProcessStartInfo(rootedPath)
			{
				UseShellExecute = false,
				WorkingDirectory = dir
			};
			
			if (arguments_ != null)
				info.Arguments = arguments_;

			return info;
		}

		static string ensurePathRooted(string pathOrFilename)
		{
			if (Path.IsPathRooted(pathOrFilename))
				return pathOrFilename;

			var thisLocation = Assembly.GetExecutingAssembly().Location;
			var dirThis = Path.GetDirectoryName(thisLocation);
			Debug.Assert(dirThis != null);
			return Path.Combine(dirThis, pathOrFilename);
		}
	}
}
