using System;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	/**
		Partial reconcilation works on ISyncItems and supports the reconcilation of one 
		replica.

		The sync items are modified in the process to reflect the current state of synchronziation.
	**/

	sealed class PartialReconciler : ReconcilerBase
	{
		readonly IRelocator Relocator;

		public PartialReconciler(IRelocator relocator)
		{
			Relocator = relocator;
		}

		public void reconcile(ISyncItem item, uint target, IReconcilePolicy policy)
		{
			reconcile(item, null, target, policy);
		}

		void reconcile(ISyncItem item, IScope parentScope_, uint target, IReconcilePolicy policy)
		{
			Debug.Assert(target == 0 || target == 1);

			var source = target ^ 1;

			// process all the items where we need to reconcile and source is the prototype!
			// before processing

			bool early = false;

			if (item.State == SyncItemState.Reconcile && item.ReconcileIndex.Value == source && item.isEarlyReconcile())
			{
				early = true;

				policy.tryReconcile(parentScope_, item,
					() => Relocator.reconcile(item.Items, source));
			}

			if (item.Nested.Any())
			{
				var currentScope = parentScope_ != null ? parentScope_.enter(item.Name) : Scope.Root;

				using (Relocator.push(currentScope))
				{
					foreach (var nested in item.Nested)
					{
						reconcile(nested, currentScope, target, policy);
					}
				}
			}

			if (item.State != SyncItemState.Reconcile || item.ReconcileIndex.Value != source || early)
				return;

			policy.tryReconcile(parentScope_, item, 
				() => Relocator.reconcile(item.Items, source));
		}

	}

	static class SyncItemExtensions
	{
		public static bool isEarlyReconcile(this ISyncItem item)
		{
			Debug.Assert(item.State == SyncItemState.Reconcile);

			switch (item.SubState)
			{
				case SyncItemSubState.ReconcileCreate:
					return true;

					// when an update item has nested, it must be updated first.
				case SyncItemSubState.ReconcileUpdate:
					return item.Items[item.ReconcileIndex.Value].Nested.Count != 0;

				case SyncItemSubState.ReconcileDelete:
					return false;
			}

			Debug.Assert(false);
			return false;
		}
	}
}
