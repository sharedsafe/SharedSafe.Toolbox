// #define NEW_DELETE_CONFLICT

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	sealed class VirtualReconciler : ReconcilerBase
	{
		/**
			Implementation of the core reconcilation algorithm for two replicas.

			It returns the root item of the new original knowledge.
		**/

		public ISyncItem reconcile(IKnowledge[] fs, IDirtyPath[] dirty)
		{
			Debug.Assert(fs.Length == 2 && dirty.Length == 2);
			return reconcile(null, (from f in fs select f.RootItem).ToArray(), dirty);
		}

		ISyncItem reconcile(IScope parentScope, IItem[] items, IDirtyPath[] dirty)
		{
			// note: items may not be set here, in which case they do not exist on the 
			// other side...

			Debug.Assert(items.Length == 2 && dirty.Length == 2);

			// if both are not dirty, left equals right and we don't need to do anything, return
			// (dirty is up-closed)

			if (dirty[0] == null && dirty[1] == null)
			{
				// if none is dirty both items must exist!
				Debug.Assert(items[0] != null && items[1] != null);

				// this is special, because we are required to add all children which have changed:
				return createNoChangeSyncItem(items);
			}

			// if both items are not set, we can not resolve the responsible reconcilation 
			// algorithm, but we know that both sides have been deleted, so there is also nothing to sync.

			if (items[0] == null && items[1] == null)
			{
				recordSynchronicity("Content deleted in both replicas", parentScope, items);

				/// one of the dirties must be set
				Debug.Assert(dirty[0] != null || dirty[1] != null);
				var name = dirty[0] != null ? dirty[0].Name : dirty[1].Name;
				return new SyncItem(name, items, SyncItemState.InSync, SyncItemSubState.Deleted);
			}

#if NEW_DELETE_CONFLICT
			var c = ReconcilerHelper.classify(items, dirty);

			// check for update/delete conflicts (we can not handle them)
			// (both dirty, but only one item set)

			if (c == 2 || c == 5)
			{
				Log.D("detected update / delete conflict");
				recordConflict("Update / Delete conflict", parentScope, items);
				return new SyncItem(items, SyncItemState.Conflict, SyncItemSubState.ConflictUpdateDelete);
			}
#endif

			// note: scope may be null in case of root folders
			Debug.Assert(items.Length == 2);
			Debug.Assert(items[0] != null || items[1] != null);
			Debug.Assert(items[0] == null || items[1] == null || items[0].Name == items[1].Name);

			var baseItem = items[0] ?? items[1];

			return reconcileInternal(
				parentScope == null ? Scope.Root : parentScope.enter(baseItem.Name),
				items, dirty);
		}


		ISyncItem reconcileInternal(IScope nestedScope, IItem[] items, IDirtyPath[] dirty)
		{
			// one of them must be dirty (no dirty no call)!
			Debug.Assert(
				nestedScope != null &&
				items.Length == 2 &&
				(items[0] != null || items[1] != null) &&
				dirty.Length == 2 &&
				(dirty[0] != null || dirty[1] != null));

			// warning: scope may be null, if we are processing root folders!
			var parentScope = nestedScope.Parent_;

			// names must be equal!
			Debug.Assert(items[0] == null || items[1] == null || items[0].Name == items[1].Name);

			// both are directories, so we must recurse anyway.

			// the original states that we should do this is lexical order, but I 
			// think this is not required, so we process them as they come out from the
			// dictionary.

			var nested = ReconcilerHelper.combineNestedItems(items[0], items[1]);

			// three operation modes:

			// 0, 4: new
			// 1, 3: delete
			// 6, 7, 8: update

			var operation = ReconcilerHelper.classify(items, dirty);

			ISyncItem resultPrototype = null;

			//
			// Structure reconcillation
			//

			switch (operation)
			{
				// new (create item):
				case 0:
				case 4:
#if !NEW_DELETE_CONFLICT
				// new / delete (no conflict => but the new item survives, alternative would be a conflict,
				// but when we have no UI item for resolving it, it does not make sense, so we create it.
				case 2:
				case 5:
#endif
					{
						var i = items[0] != null ? 0u : 1u;
						resultPrototype = new SyncItem(items, SyncItemSubState.ReconcileCreate, i);
					}
					break;


				// one side update (copy item or attributes):
				case 6:
				case 7:
					{
						int dominantIndex = dirty[0] != null ? 0 : 1;

						// if the dominant side contains nested items, we need
						// to process the update _before_ nested items are processed.

						// note: this is basically a hack to support file => folder and folder => file
						// mutations, but won't support different types of folder mutations :(

						if (items[dominantIndex].Nested.Count != 0)
							resultPrototype = new SyncItem(items, SyncItemSubState.ReconcileUpdate, dominantIndex.unsigned());
					}
					break;


				// dual side update (merge attributes if they differ, but _after_ we processed nested, see below)
				case 8:
					break;
			}

			//
			// Content reconcillation
			//

			var nestedItems = new List<ISyncItem>();

			foreach (var pair in nested)
			{
				var name = pair.Key;
				var value = pair.Value;

				var ld = dirty[0] != null ? ReconcilerHelper.resolveNestedDirty(dirty[0], name) : null;
				var rd = dirty[1] != null ? ReconcilerHelper.resolveNestedDirty(dirty[1], name) : null;


				var nestedTwo = value.ToArray();
				var item = reconcile(nestedScope, nestedTwo, new[] {ld, rd});

				if (item != null)
					nestedItems.Add(item);
				// else item is not stable
			}

			// note: in several states, the resulting prototype may not be set yet.
			Debug.Assert(resultPrototype != null
				|| operation == 8
				|| operation == 1 || operation == 3
				|| operation == 6 || operation == 7);

			switch (operation)
			{
				// 1,3: delete
				case 1:
				case 3:
					{
						int i = items[0] == null ? 0 : 1;
						// todo: if we can not get rid of the directory, we essentially have a consistency problem,
						// we may simply ignore this (believing that the next scan detects the directory)
						// todo: this must be a test case!
						// todo: log result (directory could not be deleted :(

						recordDeleted("Deleted", parentScope, items);
						resultPrototype = new SyncItem(items, SyncItemSubState.ReconcileDelete, i.unsigned());
					}
					break;

				// one side update (copy item or attributes):
				case 6:
				case 7:
					{
						int dominantIndex = dirty[0] != null ? 0 : 1;

						if (items[dominantIndex].Nested.Count == 0)
							resultPrototype = new SyncItem(items, SyncItemSubState.ReconcileUpdate, dominantIndex.unsigned());
						// else resultPrototype has been set above!
					}
					break;

				case 8:
					// both items are existing, update/update is currently running, we want to merge 
					// after content update has been finished.

					// note: may return null for root folders or if attributes could not be merged.

					resultPrototype = new SyncItem(items, SyncItemState.NeedsMerge, SyncItemSubState.NeedsMerge);

					break;
			}

			Debug.Assert(resultPrototype != null);
			resultPrototype.add(nestedItems);
			return resultPrototype;
		}


		#region Helper

		static ISyncItem createNoChangeSyncItem(IItem[] items)
		{
			Debug.Assert(items.Length == 2 && items[0] != null && items[1] != null);

			if (items[0].Nested.Count != items[1].Nested.Count)
				throw new InternalError("No Change Items have different nested counts", items);
			
			var si = new SyncItem(items, SyncItemState.InSync, SyncItemSubState.NoChange);

			var nested = ReconcilerHelper.combineNestedItems(items[0], items[1]);
			if (nested.Count != items[0].Nested.Count)
				throw new InternalError("NoChange: combined nested items have different nested counts", items);

			foreach (var n in nested)
				// recurse!
				si.add(createNoChangeSyncItem(n.Value.ToArray()));

			return si;
		}

		#endregion

	}
}
