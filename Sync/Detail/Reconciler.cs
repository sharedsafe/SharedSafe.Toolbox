// #define NEW_DELETE_CONFLICT

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	/**
		Implementation of the main reconcilation algorithm.

		Note: this is not even required for SharedSafe. 
		SharedSafe uses VirtualReconciler
	**/

	sealed class Reconciler : ReconcilerBase
	{
		readonly IRelocator Relocator;
		
		public Reconciler(IRelocator relocator)
		{
			Relocator = relocator;
		}

		/**
			Implementation of the core reconcilation algorithm for two replicas.

			It returns the root item of the new original knowledge.
		**/

		public IItem reconcile(IKnowledge[] fs, IDirtyPath[] dirty)
		{
			Debug.Assert(fs.Length == 2 && dirty.Length == 2);
			return reconcile(null, (from f in fs select f.RootItem).ToArray(), dirty);
		}

		IItem reconcile(IScope parentScope_, IItem[] items, IDirtyPath[] dirty)
		{
			// note: items may not be set here, in which case they do not exist on the 
			// other side...

			Debug.Assert(items.Length == 2 && dirty.Length == 2);

			// if both are not dirty, left equals right and we don't need to do anything, return
			// (dirty is up-closed)

			if (dirty[0] == null && dirty[1] == null)
				return items[0];

			// if both items are not set, we can not resolve the responsible reconcilation 
			// algorithm, but we know that both sides have been deleted, so there is also nothing to sync.

			if (items[0] == null && items[1] == null)
			{
				recordSynchronicity("Content deleted in both replicas", parentScope_, items);
				return null;
			}

#if NEW_DELETE_CONFLICT

			var c = ReconcilerHelper.classify(items, dirty);
	
			// check for update/delete conflicts (we can not handle them)
			// (both dirty, but only one item set)

			// update: it does not seem to make sense here to create a conflict if
			// A: does not care about a file and
			// B: has an update
			//
			// the logical next step is for A to get the new file and not to
			// create a conflict
			// see also VirtualReconciler

			if (c == 2 || c == 5)
			{
				Log.D("detected update / delete conflict");
				recordConflict("Update / Delete conflict", parentScope_, items);
				return null;
			}
#endif

			// note: scope may be null in case of root folders
			Debug.Assert(items.Length == 2);
			Debug.Assert(items[0] != null || items[1] != null);
			Debug.Assert(items[0] == null || items[1] == null || items[0].Name == items[1].Name);


			return reconcileInternal(parentScope_, items, dirty);
		}


		IItem reconcileInternal(IScope parentScope_, IItem[] items, IDirtyPath[] dirty)
		{
			// one of them must be dirty (no dirty no call)!
			Debug.Assert(
				items.Length == 2 &&
				(items[0] != null || items[1] != null) &&
				dirty.Length == 2 && 
				(dirty[0] != null || dirty[1] != null));

			// names must be equal!
			Debug.Assert(items[0] == null || items[1] == null || items[0].Name == items[1].Name);

			// both are directories, so we must recurse anyway.

			// the original states that we should do this in lexical order, but I 
			// think this is not required, so we process them as they come out from the
			// dictionary.

			var nested = ReconcilerHelper.combineNestedItems(items[0], items[1]);

			// three operation classes:

			// 0, 4: new
			// 1, 3: delete
			// 6, 7, 8: update

			var operation = ReconcilerHelper.classify(items, dirty);

			IItem resultPrototype = null;

			//
			// Structure reconcillation
			//

			switch (operation)
			{
					// new (create item):
				case 0:
				case 4:
#if !NEW_DELETE_CONFLICT
					// delete / new
				case 2:
				case 5:
#endif
					{
						uint i = items[0] != null ? 0u : 1u;
						if (!Relocator.tryReconcile(items, i))
							return null;

						resultPrototype = items[i];
					}
					break;


					// one side update (copy item or attributes):
				case 6:
				case 7:
					{
						uint dominantIndex = dirty[0] != null ? 0u : 1u;

						// if the dominant side contains nested items, we need
						// to process the update _before_ nested items are processed.

						// note: this is basically a hack to support file => folder and folder => file
						// mutations, but won't support different types of folder mutations :(

						if (items[dominantIndex].Nested.Count != 0)
						{
							if (!Relocator.tryReconcile(items, dominantIndex))
							{
								recordInconsistency("Failed to synchronize", parentScope_, items);
								return null;
							}

							resultPrototype = items[dominantIndex];
						}
					}
					break;


					// dual side update (merge attributes if they differ, but _after_ we processed nested, see below)
				case 8:
					break;
			}

			//
			// Content reconcillation
			//

			var nestedItems = new List<IItem>();

			if (nested.Count != 0)
			{
				var baseItem = items[0] ?? items[1];
				var nestedScope = parentScope_ != null ? parentScope_.enter(baseItem.Name) : Scope.Root;

				using (Relocator.push(nestedScope))
				{
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
				}
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
						uint i = items[0] == null ? 0u : 1u;
						// todo: if we can not get rid of the directory, we essentially have a consistency problem,
						// we may simply ignore this (believing that the next scan detects the directory)
						// todo: this must be a test case!
						// todo: log result (directory could not be deleted :(

						Relocator.tryReconcile(items, i);
						recordDeleted("Deleted", parentScope_, items);
						return null;
					}

					// one side update (copy item or attributes):
				case 6:
				case 7:
					{
						uint dominant = dirty[0] != null ? 0u : 1u;

						if (items[dominant].Nested.Count == 0)
						{
							if (!Relocator.tryReconcile(items, dominant))
							{
								recordInconsistency("Failed to synchronize", parentScope_, items);
								return null;
							}

							resultPrototype = items[dominant];
						}
					}
					break;

				case 8:
					// both items are existing, update/update is currently running, we want to merge 
					// after content update has been finished.

					// note: may return null for root folders or if attributes could not be merged.

					resultPrototype = Relocator.merge(items);
					// if attributes could not be merged, we exit (merge was not successful, remerging is required)
					// todo: verify this behavior, we could easily live with that.
					// note: later we may not handle FileFlags and LastModificationTime for folder anyway, so
					// this may never happen.

					if (resultPrototype == null)
					{
						recordConflict("Update / Update conflict", parentScope_, items);
						return null;
					}

					break;
			}

			Debug.Assert(resultPrototype != null);

			// clone result without nested items (result is only the prototype)
			// (note: file system is detached here)
			resultPrototype = resultPrototype.copyShallow();

			// todo: may optimize this with addrange?
			foreach (var ni in nestedItems)
				resultPrototype.addNested(ni);

			return resultPrototype;
		}
	}
}
