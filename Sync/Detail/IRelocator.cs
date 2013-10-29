using System;

namespace Toolbox.Sync.Detail
{
	/**
		A relocator is actively involved in item content processing.

		It knows the replicas it is working on and supports a number of
		content operations which are all meant to complete successfully or not, 
		but are not allowed to throw exceptions in a case that can be predicted.

		The reconcilation (or merges) are processed in a strict hierarchical order.
		The current point in the hierarhcy is set using push() / pop().
	**/

	interface IRelocator
	{
		IReplica[] Replicas { get; }
		SyncOptions Options { get; }


		/**
			Push the current scope.
		**/

		IDisposable push(IScope scope);

		/**
			Copy one item (specified by index) over all the other items.

			Items with index != index may be null, in which case the target is not expected 
			yet to exist!

			If the item with index == null then (unchanged) other items shall be deleted.
		
			Fail (return false) if

			a) if one of the target items have been changed
			b) if copying was not successful
			c) if target is null but there _is_ actually something existing at the target location.

			If this function fails, it should be guaranteed that no harm is done,
			meaning that

			a) no changes have been made to the replicas
			b) temporary data is all deleted
		**/

		bool tryReconcile(IItem[] items, uint index);

		void reconcile(IItem[] items, uint index);

		/**
			Merge attributes.

			Returns the merged item or null if merging was not possible.
		**/

		IItem merge(IItem[] items);
	}
}
