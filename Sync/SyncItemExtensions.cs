using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System;

namespace Toolbox.Sync
{
	public static class SyncItemExtensions
	{
		public static string dumpNotInSync(this ISyncItem item)
		{
			return item.dump(i => i.State != SyncItemState.InSync);
		}

		public static string dump(this ISyncItem item)
		{
			return item.dump(i => true);
		}

		public static string dump(this ISyncItem item, Func<ISyncItem, bool> filter)
		{
			var sb = new StringBuilder();
			dump(item, filter, sb);
			return sb.ToString();
		}

		static void dump(ISyncItem item, Func<ISyncItem, bool> filter, StringBuilder builder)
		{
			dump(item, filter, builder, string.Empty);
		}

		static void dump(ISyncItem item, Func<ISyncItem, bool> filter, StringBuilder builder, string prefix)
		{
			if (filter(item))
				builder.AppendLine(prefix + item);

			var nested = (from i in item.Nested orderby i.State, item.SubState select i).ToArray();
			if (nested.Length == 0)
				return;

			var nestedPrefix = prefix + "    ";

			foreach (var i in nested)
				dump(i, filter, builder, nestedPrefix);
		}

		
		// note '/' separated :(

		public static ISyncItem tryLocatePath(this ISyncItem item, string path)
		{
			if (!path.Contains("/"))
				return item.tryLocate(path);

			string[] components = path.Split('/');
			return item.tryLocatePath(components);
		}


		public static ISyncItem tryLocatePath(this ISyncItem item, IEnumerable<string> components)
		{
			foreach (var component in components)
			{
				item = item.tryLocate(component);
				if (item == null)
					return null;
			}

			return item;
		}



		public static void add(this ISyncItem item, IEnumerable<ISyncItem> multiple)
		{
			foreach (var nested in multiple)
				item.add(nested);
		}

		public static bool isTreeInSync(this ISyncItem root)
		{
			foreach (var item in root.traversePreOrdered())
				if (item.State != SyncItemState.InSync)
					return false;

			return true;
		}

		/**
			Note: because of subsequent error handling, a local or remote item tree may differ 
			(though we don't create a remote item tree yet).
		**/

		public static IItem tryMakeStableItemTree(this ISyncItem root, int repositoryIndex)
		{
			var item = tryMakeStableItem(root, repositoryIndex);
			if (item == null)
				return null;

			foreach (var nested in root.Nested)
			{
				var ni = nested.tryMakeStableItemTree(repositoryIndex);
				if (ni != null)
					item.addNested(ni);
			}

			return item;
		}

		static IItem tryMakeStableItem(ISyncItem item, int repositoryIndex)
		{
			switch (item.State)
			{
				case SyncItemState.InSync:

					switch (item.SubState)
					{
						case SyncItemSubState.NoChange:
							{
								Debug.Assert(item.Items[0] != null && item.Items[1] != null);
								var proto = item.Items[repositoryIndex];
								return proto.copyShallow();
							}

						case SyncItemSubState.Reconciled:
							{
								var proto = item.Items[item.ReconcileIndex.Value];
								return proto != null ? proto.copyShallow() : null;
							}

						case SyncItemSubState.Merged:
							{
								var proto = item.Items[repositoryIndex];
								return proto.copyShallow();
							}

							// deleted can not get a stable entry, so that we see new files 
							// with same names then.
						case SyncItemSubState.Deleted:
							return null;
					}
					break;

				case SyncItemState.Reconcile:

					switch (item.SubState)
					{
						// a delete failed, in this case, the local item stays stable, so that
						// the error is reconstructed in the next round.

						case SyncItemSubState.ReconcileDelete:
							var prototype = item.Items[repositoryIndex];
							if (prototype != null)
								return prototype.copyShallow();
							break;

						// a local overwrite failed, in this case we also keep the 
						// local item stable, so that error comes up next time.
						
						case SyncItemSubState.ReconcileUpdate:
							if (item.ReconcileIndex != repositoryIndex && item.Items[repositoryIndex] != null)
								return item.Items[repositoryIndex].copyShallow();
							break;
					}
					break;
			}

			return null;
		}

		public static IItem tryMakeItemTree(this ISyncItem root, int i)
		{
			Debug.Assert(i < root.Items.Length);

			var item = root.Items[i];
			if (item == null)
				return null;

			item = item.copyShallow();
			foreach (var nested in root.Nested)
			{
				var ni = nested.tryMakeItemTree(i);
				if (ni != null)
					item.addNested(ni);
			}

			return item;
		}

		/**
			Try merge items, mark conflict if not mergable.
			Right now, this considers all merges a Update/Update conflict.
		**/

		public static void tryMergeOrMarkConflict(this ISyncItem syncItem)
		{
			if (syncItem.State != SyncItemState.NeedsMerge)
				return;

			Debug.Assert(syncItem.SubState == SyncItemSubState.NeedsMerge);
			// both must be set on merge!

			var left = syncItem.Items[0];
			var right = syncItem.Items[1];

			Debug.Assert(left != null && right != null);
			
			if (canMerge(left, right))
				syncItem.merged(left);
			else
				syncItem.markMergeConflict();
		}


		static bool canMerge(IItem left, IItem right)
		{
			var t = left.Type;
			if (t != right.Type)
				return false;

			switch (t)
			{
				// root folders are always merged
				case ItemType.RootFolder:
					return true;

				case ItemType.Folder:
					// right now we can merge folders, because we do not compare folder's
					// dates and flags!
					// ==> this should be somehow decided by meta data (IItemType or so)
					return true;

				case ItemType.File:
					return canMergeFile(left, right);

			}

			return false;
		}

		static bool canMergeFile(IItem left, IItem right)
		{
			Debug.Assert(left.Type == ItemType.File && right.Type == ItemType.File);
			var la = (FileAttributes) left.Attributes;
			var ra = (FileAttributes) right.Attributes;
			return FileAttributesHashComparer.equalBasedOnHash(la, ra);
		}

		/**
			Try to remove all redundant updates.

			Right now these are folder updates 
			(because dirty paths are up-closed?) 
			if they compare equal.. which they always do right now.

			This shall not happen for files, because

			O != A
			O == B
			always results in A != B
			
			where 
				O: stable File
				A: updated repository
				B: unchanged repository

			tbd: make a test case for this which blows when we remove the reconcilation here!
		**/

		public static bool canReconcileRedundantUpdate(this ISyncItem item, SyncOptions options)
		{
			return item.State == SyncItemState.Reconcile &&
				item.SubState == SyncItemSubState.ReconcileUpdate &&
					options.areItemsEqual(item.Items[0], item.Items[1]);
		}

		/**
			Returns true fi there are any conflicts or merges in the tree.
		**/

		public static bool anyConflictsOrMerges(this ISyncItem item)
		{
			return item.traversePreOrdered().Any(si => si.State == SyncItemState.Conflict || si.State == SyncItemState.NeedsMerge);
		}

		/**
			Resolve all conflicts in the tree beginning from that node.
		**/

		public static void resolveAllConflicts(this ISyncItem item, uint repositoryIndex)
		{
			foreach (var i in item.traversePostOrdered())
			{
				if (i.State != SyncItemState.Conflict)
					continue;

				item.resolveConflict(repositoryIndex);
			}
		}
	}
}