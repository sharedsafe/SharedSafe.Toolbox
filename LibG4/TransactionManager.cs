using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox;

namespace LibG4
{
	/**
		The transaction manager is a per thread static class to schedule and run transactions.
	**/

	sealed class TransactionManager
	{
		[ThreadStatic]
		static TransactionManager TM;

		List<Transaction> _queue = new List<Transaction>();
		readonly Stack<Transaction> _runningTransactions = new Stack<Transaction>();
		readonly IDisposable _exit;
		bool _updating;

		TransactionManager()
		{
			_exit = new DisposeAction(exit);
		}

		public static void update()
		{
			CurrentTM.updateInternal();
		}

		void updateInternal()
		{
			// note: this might call back (Other system event handlers may be called, which call back to us :(

			if (_updating)
				return;

			_updating = true;

			try
			{
				while (_queue.Count != 0)
				{
					var runNow = _queue;
					_queue = new List<Transaction>();

					runNow.ForEach(t => t.run());
				}
			}
			finally
			{
				_updating = false;
			}
		}


		#region Public Statics

		public static void schedule(Transaction transaction)
		{
			CurrentTM.scheduleInternal(transaction);
		}

		// note: this should be optimized, it is called for each property read!

		public static Transaction CurrentTransaction
		{
			get
			{
				return CurrentTM.CurrentTransactionInternal;
			}
		}

		public static IDisposable enter(Transaction transaction)
		{
			return CurrentTM.enterInternal(transaction);
		}

		#endregion

		#region Instance

		void scheduleInternal(Transaction transaction)
		{
			_queue.Add(transaction);
		}

		Transaction CurrentTransactionInternal
		{
			get 
			{
				if (_runningTransactions.Count == 0)
					return null;


				return _runningTransactions.Peek();
			}
		}

		IDisposable enterInternal(Transaction transaction)
		{
			_runningTransactions.Push(transaction);
			return _exit;
		}

		void exit()
		{
			Debug.Assert(_runningTransactions.Count != 0);
			_runningTransactions.Pop();
		}


		#endregion
		
		static TransactionManager CurrentTM
		{
			get
			{
				TransactionManager tm = TM;
				if (tm == null)
				{
					tm = new TransactionManager();
					TM = tm;
				}

				return tm;
			}
		}
	}
}
