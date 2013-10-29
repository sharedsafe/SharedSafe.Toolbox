using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox;

namespace LibG4
{
	/**
		A live transaction.
	**/

	public sealed class Transaction : IDisposable
	{
		Action _action;

		/// Scheduled for rerun?
		bool _scheduled;
		bool _tracking;
		int _readIndex;
		// note: this could be an array, I think!
		List<List<Transaction>> Reads = new List<List<Transaction>>();

		// and this could be chained actions.
		List<IDisposable> _attachments;

		public Transaction(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			// this actually may happen :( Testcase is the Account Configuration in SharedSafe
			// Debug.Assert(!_scheduled);
			// todo: shouldn't we remove it then?
			
			foreach (var transactions in Reads)
				if (!transactions.Remove(this))
					throw new Exception("internal error, removing a transaction from a reader list failed");

			Reads = null;
			_action = null;

			if (_attachments != null)
			{
				_attachments.ForEach(d => d.Dispose());
				_attachments = null;
			}
		}

		public void run()
		{
			Debug.Assert(_scheduled);
			_scheduled = false;

			if (_action == null)
			{
				Log.W("transaction called after dispose");
				return;
			}

			using (TransactionManager.enter(this))
			{
				_tracking = true;
				_readIndex = 0;

				_action();

				if (_readIndex == Reads.Count)
					return;

				for (int i = _readIndex; i != Reads.Count; ++i)
					if (!Reads[i].Remove(this))
						throw new Exception("internal error, removing a transaction from a reader list failed");

				Reads.RemoveRange(_readIndex, Reads.Count - _readIndex);
			}

			if (_scheduled)
				throw new Exception("Recursive transaction dependency detected: transaction rescheduled while running");
		}

		public void stopTracking()
		{
			_tracking = false;
		}

		public void notifyRead(List<Transaction> property)
		{
			if (!_tracking)
				return;

			var ri = _readIndex++;

			if (ri == Reads.Count)
			{
				Reads.Add(property);
				property.Add(this);
				return;
			}

			var prev = Reads[ri];
			if (prev == property)
				return;

			if (!prev.Remove(this))
				throw new Exception("internal error, removing a transaction from a reader list failed");

			Reads[ri] = property;
			property.Add(this);
		}

		public void schedule()
		{
			if (_scheduled)
				return;
			TransactionManager.schedule(this);
			_scheduled = true;
		}

		public void notifyWrite()
		{
			schedule();
		}

		#region Attachments

		public void attach(IDisposable disposable)
		{
			if (_attachments == null)
				_attachments = new List<IDisposable>();

			_attachments.Add(disposable);
		}

		#endregion
	}
}
