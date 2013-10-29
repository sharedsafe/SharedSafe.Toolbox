using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Toolbox.Sync.Detail
{
	static class DiffAlgorithm
	{
		public static void compare(IScope scope, IItem source, IItem target, IDiffCollector diffs)
		{
			Debug.Assert(source.Name == target.Name);
			if (source.Type == target.Type)
				diffs.compare(scope, source, target);
			else
			// if types are different, we mark them as diffs!
			{
				if (scope == null)
					throw new Exception("Differing root types are not supported");

				// new: instead of type change, we record everything from source deleted and in target
				// recreated!

				diffs.collectDeepDelete(scope, source);
				diffs.collectDeepNew(scope, target);
			}
		}	

		/**
			Default implementation for nested items. 
		
			May be called from IItemType implementations.

			@param scope
				Scope may be null, in which case sourceContainer and targetContainer's are roots
		**/

		public static void compareNested(IScope scope, IItem sourceContainer, IItem targetContainer, IDiffCollector diffs)
		{
			// build up lookup table for source

			var sourceItems = new Dictionary<string, IItem>();
			foreach (var item in sourceContainer.Nested)
				sourceItems.Add(item.Name, item);

			var nestedScope = scope == null ? Scope.Root : scope.enter(sourceContainer.Name);

			// go through target and try to match

			foreach (var target in targetContainer.Nested)
			{
				var name = target.Name;
				IItem source;

				if (sourceItems.TryGetValue(name, out source))
				{
					// already existing, direct compare

					compare(nestedScope, source, target, diffs);
					sourceItems.Remove(name);
					continue;
				}

				// source item is not existing, mark as new including all children

				diffs.collectDeepNew(nestedScope, target);
			}

			// source items that have not been deleted yet (not diffed), are
			// marked as diffs (they are not existing in the target anymore).

			foreach (var source in sourceItems)
				diffs.collectDeepDelete(nestedScope, source.Value);
		}


		#region Deep DiffAlgorithm Collection

		static void collectDeepNew(this IDiffCollector diff, IScope scope, IItem item)
		{
			// first the item itsself
			diff.collect(scope, item.Name, DiffKind.New);

			// then all nested

			var scopeNested = scope.enter(item.Name);

			// todo: short circuit if Nested is not supported here!

			foreach (var nested in item.Nested)
				diff.collectDeepNew(scopeNested, nested);
		}

		static void collectDeepDelete(this IDiffCollector diff, IScope scope, IItem item)
		{
			// first all nested
			// todo: short circuit if Nested is not supported here!

			var scopeNested = scope.enter(item.Name);

			foreach (var nested in item.Nested)
				diff.collectDeepDelete(scopeNested, nested);

			// then the item itsself.
			diff.collect(scope, item.Name, DiffKind.Deleted);
		}

		#endregion

	}
}
