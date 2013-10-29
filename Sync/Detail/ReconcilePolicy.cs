using System;
using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	public sealed class DefaultReconcilePolicy : IReconcilePolicy
	{
		public void confirm(IScope parentScope_, ISyncItem syncItem)
		{
			syncItem.reconciled();
		}

		public void error(IScope parentScope_, ISyncItem syncItem, Exception error)
		{
			syncItem.setReconcilationError(error);
		}

		public static readonly IReconcilePolicy Instance =
			new DefaultReconcilePolicy();
	}
}