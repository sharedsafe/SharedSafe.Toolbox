#if false
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Toolbox.IO
{
	public static class FileOperations
	{
		/**
			Why: see http://www.replicator.org/content/filedelete-does-not

			Delete a file and trow an exception after a a certain timeout, if it is not possible. 
			Set timeout to null to wait forever.

			Delete does guarantee that the file in question gets deleted, but does not guarantee that there
			is another (new) file stored at the same position upon return of the function.

			Initially I tried to make the delete more secure by using FileOptions.DeleteOnClose, but this
			behaves exactly like Delete, because you can not be sure that file wasn't opened before!!!

			So a secure version of delete, must use File.Exists. But then we can not be sure that another, new
			file was created by someone else (which is acceptable)

			Additionally to the usual Exceptions File.Delete throws, this method may throw a TimeoutException 
			if timeout is not null.
		**/

		public static void safeDelete(string filename, uint? timeout)
		{
			var sw = new Stopwatch();
			sw.Start();

			int wait = -1;

			for(;;)
			{
				try
				{
					if (!File.Exists(filename))
						return;

					File.Delete(filename);

					if (!File.Exists(filename))
						return;
				}

				catch (UnauthorizedAccessException unauthorizesAccessException)
				{
					throw;
				}

				if (timeout != null && sw.ElapsedMilliseconds >= timeout.Value)
					throw new TimeoutException("Failed to delete file {0} after {1} milliseconds".format(filename, timeout.Value));

				// we start with not waiting then 0, then 1 and then we double until 128 milliseconds and stay there 
				// (10 fs accesses per second shouldnt trouble the system much).

				switch (wait)
				{
					case -1:
						++wait;
						break;
					case 0:
						Thread.Sleep(0);
						++wait;
						break;

					default:
						Thread.Sleep(wait);
						if (wait != 128)
							wait *= 2;
						break;
				}
			}
		}
	}
}
#endif
