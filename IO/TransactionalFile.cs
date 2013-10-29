// this was once used by the ProductLicenser.exe but got replaced by LockFile

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Toolbox.IO
{
	/**
		Transactional file semantics based on the assumption that File.Move in the same directory is atomic.

		Note: uses the files [filename].tmp and [filename].lock
	**/

	public sealed class TransactionalFile : IDisposable
	{
		public readonly string Filename;
		readonly FileStream _lockFile;

		// pollMS: number of times to wait before retry
		// pollTimeoutMS: when to timeout? (null: never)

		public static TransactionalFile tryLock(string filename, uint waitBeforeRetryMS, uint? timeoutMS)
		{
			Debug.Assert(waitBeforeRetryMS != 0 && (timeoutMS == null || timeoutMS.Value != 0));

			var lockFilename = makeLockFilename(filename);

			FileStream lockFile = null;

			ulong waited = 0;
			while (lockFile == null)
			{
				Exception e;
				try
				{
					// we could use DeleteOnClose, but are scared right now.
					lockFile = File.Open(lockFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
					break;
				}
				// this happens also (see threading test)
				catch (UnauthorizedAccessException ua)
				{
					e = ua;
				}
				catch (IOException io)
				{
					e = io;
				}

				if (timeoutMS != null && waited > timeoutMS.Value)
				{
					Log.D("{0}: failed to lock, maximum waiting time exceeded".format(lockFilename));
					return null;
				}

				Log.D("{0}: {1}, waiting {2}ms".format(lockFilename, e.Message, waitBeforeRetryMS));

				Thread.Sleep(waitBeforeRetryMS.signed());
				waited += waitBeforeRetryMS;
			}

			try
			{
				var bak = makeBakFilename(filename);
				if (!File.Exists(filename) && !File.Exists(bak))
				{
					// can not recover, file was never existing before.
					lockFile.Close();
					return null;
				}

				return new TransactionalFile(filename, lockFile);
			}
			catch (Exception)
			{
				lockFile.Close();
				throw;
			}
		}

		internal TransactionalFile(string filename, FileStream lockFile)
		{
			Filename = filename;
			_lockFile = lockFile;

			recoverAndCleanup(Filename);
		}

		public void read(Action<string> readFile)
		{
			readFile(Filename);
		}

		public void write(Action<string> writeFile)
		{
			recoverAndCleanup(Filename);

			var bak = makeBakFilename(Filename);

			File.Copy(Filename, bak);
			var tmp = makeTempFilename(Filename);

			writeFile(tmp);
			File.Delete(Filename);

			if (File.Exists(Filename))
			{
				Debug.Assert(false);
			}

			File.Move(tmp, Filename);

			File.Delete(bak);
		}

		public void Dispose()
		{
			_lockFile.Close();
			var lfn = makeLockFilename(Filename);

			// try to delete lock-file (if it is locked again, IOException is thrown, see tests)
			try
			{
				File.Delete(lfn);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		static void recoverAndCleanup(string filename)
		{
			var bakFile = makeBakFilename(filename);
			var tempFile = makeTempFilename(filename);

			if (!File.Exists(filename))
			{
				File.Move(bakFile, filename);
			}
			else
			{
				File.Delete(bakFile);
			}

			File.Delete(tempFile);
		}

		static string makeLockFilename(string filename)
		{
			return filename + ".lck";
		}

		static string makeBakFilename(string filename)
		{
			return filename + ".bak";
		}

		static string makeTempFilename(string filename)
		{
			return filename + ".tmp";
		}
	}
}
