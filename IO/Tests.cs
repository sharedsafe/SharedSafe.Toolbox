#if DEBUG
using System;
using System.IO;
using System.Threading;
using NUnit.Framework;


namespace Toolbox.IO
{
	[TestFixture]
	public class TransactionFileTest
	{
		const string fn = "TransactionFileTestFile";

		[Test]
		public void doesNotExist()
		{
			File.Delete(fn);
			var tf = TransactionalFile.tryLock(fn, 20, 200);
			Assert.Null(tf);
		}

		[Test]
		public void shallBeLocked()
		{
			File.Delete(fn);
			File.WriteAllText(fn, "testcontent");

			using (var tf = TransactionalFile.tryLock(fn, 20, 200))
			{
				Assert.NotNull(tf);

				var tf2 = TransactionalFile.tryLock(fn, 20, 200);
				Assert.Null(tf2);
			}

			using (var tf3 = TransactionalFile.tryLock(fn, 20, 200))
			{
				Assert.NotNull(tf3);
			}
		}

		[Test]
		public void lockedAndDelete()
		{
			File.Delete(fn);
			File.WriteAllText(fn, "testcontent");

			using (var tf = TransactionalFile.tryLock(fn, 20, 200))
			{
				Assert.NotNull(tf);

				// delete lockfile
				File.Delete(tf.Filename + ".lck");
			}
		}

		[Test]
		public void threadTest()
		{
			const int Duration = 60000;

			File.WriteAllText(fn, "0");

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

			var res = File.ReadAllText(fn);

			this.D("final content:" + res);
		}

		static long cnt;

		static void thread(WaitHandle abortEvent)
		{
			try
			{
				while (!abortEvent.WaitOne(0))
				{
					using (var tf = TransactionalFile.tryLock(fn, 1, null))
					{
						if (tf == null)
							continue;

						var id = Interlocked.Increment(ref cnt);
						Log.D("enter " + id);

						string content = null;
						tf.read(filename => content = File.ReadAllText(filename));
						var i = int.Parse(content);
						++i;
						tf.write(filename => File.WriteAllText(filename, i.ToString()));

						Log.D("exit" + id);
					}
				}

			}
			catch (Exception e)
			{
				Log.E(e, "Thread died");
				throw;
			}
		}
	}

	/**
		For example Microsoft Security Essentials (v2) causes the below problems.
	**/

	[TestFixture]
	public class DeleteAtomicTest
	{
		const string fn = "DeleteAtomicFile";

		[Test]
		public void deleteTest()
		{
			int failure = 0;
			for (int i = 0; i != 10000; ++i)
			{
				File.WriteAllText(fn, "test");
				File.Delete(fn);

				if (!File.Exists(fn))
					continue;

				++failure;
				while (File.Exists(fn))
					;
			}

			if (failure != 0)
				throw new Exception("Delete failures: " + failure);
		}

#if false

		// because of the UnauthorizedAccessException, we found no way yet to safely delete files :(

		[Test]
		public void safeDeleteTest()
		{
			int failure = 0;
			for (int i = 0; i != 10000; ++i)
			{
				File.WriteAllText(fn, "test");
				FileOperations.safeDelete(fn, null);

				if (!File.Exists(fn))
					continue;

				++failure;
				while (File.Exists(fn))
					;
			}

			if (failure != 0)
				throw new Exception("Delete failures: " + failure);
		}
#endif

	}
}
#endif
