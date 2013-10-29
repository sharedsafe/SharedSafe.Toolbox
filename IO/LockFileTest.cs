#if DEBUG
using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Toolbox.IO
{
	[TestFixture]
	
	public class LockFileTest
	{
		const string fn = "LockFileTestFile";

		[Test]
		public void doesNotExist()
		{
			if (File.Exists(fn))
				File.Delete(fn);
			using (var tf = LockFile.tryLock(fn))
				Assert.NotNull(tf);
		}

		[Test]
		public void exists()
		{
			File.WriteAllText(fn, "hello world");

			using (var tf = LockFile.tryLock(fn))
				Assert.NotNull(tf);
		}

		[Test]
		public void shallBeLocked()
		{
			if (File.Exists(fn))
				File.Delete(fn);

			using (var tf = LockFile.tryLock(fn))
			{
				Assert.NotNull(tf);

				using (var tf2 = LockFile.tryLock(fn))
					Assert.Null(tf2);
			}

			using (var tf3 = LockFile.tryLock(fn))
			{
				Assert.NotNull(tf3);
			}
		}

		[Test]
		public void threadTest()
		{
			const int Duration = 60000;

			if (File.Exists(fn))
				File.Delete(fn);

			var abortEvent = new ManualResetEvent(false);

			var t1 = new Thread(() => thread(abortEvent));
			var t2 = new Thread(() => thread(abortEvent));
			var t3 = new Thread(() => thread(abortEvent));
			var t4 = new Thread(() => thread(abortEvent));
			t1.Start();
			t2.Start();
			t3.Start();
			t4.Start();

			Thread.Sleep(Duration);

			abortEvent.Set();
			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();
	
			Assert.False(died);
		}

		static long cnt;
		static volatile bool died;

		static void thread(WaitHandle abortEvent)
		{
			try
			{
				while (!abortEvent.WaitOne(0))
				{
					using (var lf = LockFile.tryLock(fn))
					{
						if (lf == null)
							continue;

						var id = Interlocked.Increment(ref cnt);
						try
						{
							if (id != 1)
								throw new Exception("locked twice");
							Thread.Sleep(1);
						}
						finally
						{
							Interlocked.Decrement(ref cnt);
						}
					}
				}

			}
			catch (Exception e)
			{
				Log.E(e, "Thread died");
				died = true;
				throw;
			}
		}
	}
}
#endif
