using System;

namespace Toolbox.Sync.Detail
{
	/**
		Defines what to do with an ISyncItem when reconcilation is successful or failed.
	**/

	public interface IReconcilePolicy
	{
		void confirm(IScope parentScope_, ISyncItem syncItem);
		void error(IScope parentScope_, ISyncItem syncItem, Exception error);
	}

	static class ReconcilePolicyExtensions
	{
		public static bool tryReconcile(this IReconcilePolicy policy, IScope parentScope_, ISyncItem item, Action action)
		{
			try
			{
				action();
				policy.confirm(parentScope_, item);
				return true;
			}
			catch (Exception e)
			{
				policy.error(parentScope_, item, e);
				return false;
			}
		}
	}
}
