using System.Diagnostics;

namespace RootSE.Provider
{
	sealed class TransactionCoordinator
	{
		uint _activeTransactions;
		bool _failed;

		public void beginTransaction()
		{
			++_activeTransactions;
		}

		public void endTransaction(bool commit)
		{
			Debug.Assert(_activeTransactions != 0);
			--_activeTransactions;
			if (!commit)
				_failed = true;
		}

		public bool ShouldEndTransaction
		{
			get { return _activeTransactions == 0; }
		}

		public bool ShouldCommitTransaction
		{
			get
			{
				Debug.Assert(ShouldEndTransaction);
				return !_failed;
			}
		}
	}
}