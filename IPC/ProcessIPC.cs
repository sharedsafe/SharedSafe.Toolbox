// right now we don't support case insensitivity, because we can not support it from C++
// #define CASE_INSENSITIVE_NORMALIZATION

using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Toolbox.IPC
{
	public static class ProcessIPC
	{
		// connect to an already existing end point.
		public static IObjectEndPoint tryConnectToExisting(string path)
		{
			return ObjectEndPoint.tryConnect(path2Name(path));
		}

		public static IObjectEndPoint connect(string path, uint processStartupTimeout)
		{
			return connect(path, processStartupTimeout, null);
		}

		public static IObjectEndPoint connect(string path, uint processStartupTimeout, string processArguments_)
		{
			using (Log.pushContext("ProcessIPC"))
			{
				var name = path2Name(path);

				Log.D("{0} connecting...".format(name));
				
				var _oep = ObjectEndPoint.tryConnect(name);
				if (_oep != null)
				{
					if (processArguments_ != null)
						Log.E("Failed to pass arguments to process {0}".format(name));

					return _oep;
				}

				ProcessLauncher.launch(path, processArguments_);

				const uint WaitPerConnectionTry = 200;
				uint waitedSoFar = 0;
				uint retries = 0;

				while (waitedSoFar < processStartupTimeout)
				{
					_oep = ObjectEndPoint.tryConnect(name);
					++retries;
					if (_oep != null)
					{
						Log.D("Successfully connected after {0} retries and {1} milliseconds".format(retries, waitedSoFar));
						return _oep;
					}

					Thread.Sleep(WaitPerConnectionTry.signed());
					waitedSoFar += WaitPerConnectionTry;
				}

				throw new Exception("failed to connect to process end point");
			}
		}


		public static bool isServerListening(string processPath)
		{
			return isListening(processPath);
		}

		public static IObjectServer tryCreateServer()
		{
			return ProcessServer.tryCreate(getNormalizedEntryExecutable());
		}

		public static IObjectServer createServer()
		{
			return createServer(getNormalizedEntryExecutable());
		}

		public static IObjectServer tryCreateServer(string processPath)
		{
			return ProcessServer.tryCreate(processPath);
		}

		public static IObjectServer createServer(string processPath)
		{
			var server = ProcessServer.tryCreate(processPath);
			if (server == null)
				throw new Exception("Process IPC server already running.");
			return server;
		}

		#region Helpers to derive process names from the current executable process.

		public static string makeNormalizedProcessPath(string path)
		{
			var dir_ = tryGetNormalizedEntryExecutableDirectory();
			if (dir_ == null)
			{
				dir_ = Directory.GetCurrentDirectory();
				Log.D("Failed to get normalized entry executable directory, using current directory, which is {0}".format(dir_));
			}

			return Path.Combine(dir_, path);
		}

		public static string tryGetNormalizedEntryExecutableDirectory()
		{
			var ne_ = tryGetNormalizedEntryExecutable();
			if (ne_ == null)
				return null;

			return Path.GetDirectoryName(ne_);
		}

		public static string getNormalizedEntryExecutable()
		{
			var ex_ = tryGetNormalizedEntryExecutable();
			if (ex_ == null)
				throw new Exception("Failed to get normalized entry executable (there may be none)");
			return ex_;
		}

		public static string tryGetNormalizedEntryExecutable()
		{
			var executableOverride = Context<EntryExecutableOverride>.CurrentOrDefault;
			if (executableOverride != null)
				return executableOverride.Executable;

			// note: CodeBase uses '/' and file:// prefix, which does not mix with Path.Combine which
			// converts '/' to '\\'.

			var assembly_ = Assembly.GetEntryAssembly();
			if (assembly_ == null)
			{
				Log.D("Entry assembly is not set");
				return null;
			}

			var location = assembly_.Location;
			if (location == null)
				throw new Exception("Entry Assembly has no location");

			return location;
		}

		public static IDisposable pushEntryExecutableOverride(string path)
		{
			return Context<EntryExecutableOverride>.push(new EntryExecutableOverride { Executable = path });
		}

		sealed class EntryExecutableOverride
		{
			public string Executable;
		}

		#endregion

		public static bool isListening(string path)
		{
			var name = path2Name(path);
			var ep = ObjectEndPoint.tryConnect(name);
			if (ep == null)
			{
				Log.D("ProcessEndPoint: {0} is _not_ listening".format(name));
				return false;
			}

			Log.D("ProcessEndPoint: {0} _is_ listening".format(name));
			ep.Dispose();
			return true;
		}

		static string path2Name(string path)
		{
			return ProcessHelper.replaceInvalidFileNameCharacters(path);
		}
	}
}
