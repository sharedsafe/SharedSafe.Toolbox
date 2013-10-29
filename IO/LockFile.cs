using System;
using System.IO;

namespace Toolbox.IO
{
	/**
		Use a file to simulate a per process lock.
		File may left in the filesystem.
	**/
	
	public sealed class LockFile : IDisposable
	{
		readonly Stream _file;

		public LockFile(Stream file)
		{
			_file = file;
		}

		public void Dispose()
		{
			_file.Dispose();
		}

		public static IDisposable tryLock(string filename)
		{
			try
			{
				var stream = File.Open(
					filename,
					FileMode.OpenOrCreate,
					FileAccess.Write,
					// we could use FileShare.None with no lock, but this crashes on Mono
					FileShare.ReadWrite);

				try
				{
					stream.Lock(0, 1);
				}
				catch (Exception)
				{
					stream.Dispose();
					return null;
				}

				return new LockFile(stream);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
