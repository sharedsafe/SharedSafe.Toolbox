/**
	Specifies a context for relocation methods in IItemType implementations.
**/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	public interface IRelocationContext
	{
		SyncOptions Options { get; }

		/**
			Record a relocation error!

			A relocation error does not need to stop the reconcilation process. Reconcilation can continue with
			partial defective state.
		**/

		RelocationError recordError(
			// overview of error
			string description,
			// The items and their paths involved
			IEnumerable<Pair<IItem, string>> items,
			// the actual action that caused the error.
			Exception e_);
	}

	public static class RelocationContextExtensions
	{
		public static RelocationError recordError(this IRelocationContext context,
			string description, IItemContext item, Exception e_)
		{
			Debug.Assert(item != null);

			return context.recordError(description, item.makeAbsolutePath(), item.Item_, e_);
		}

		static RelocationError recordError(this IRelocationContext context,
			string description, string path, IItem item_, Exception e_)
		{
			return context.recordError(description, new[] { Pair.make(item_, path) }, e_);
		}
	}
}
