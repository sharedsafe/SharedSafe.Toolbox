
namespace Toolbox.Sync
{
	/// Predefined item types

	public enum ItemType : uint
	{
		File = 0,
		Folder = 1,

		RootFolder = 2
	};

	public static class ItemTypeExtensions
	{
		public static uint index(this ItemType type)
		{
			return (uint)type;
		}
	};

	/**
		ItemContext passed to IFS instances.

		We need to pass on canonical hierarchy information, so that clients can identify tree
		positioning (to properly group changes in chunks for example)
	**/

	public interface IItemType
	{
		/// The type index (for secondary lookup and type extensions)
		uint Index { get; }

		/**
			Fill up dirty list.

			DirtyPath is defined as a path difference between a and b.
		**/

		void diff(IScope scope, IItem a, IItem b, IDiffCollector diffs);

		bool equals(IItem a, IItem b, SyncOptions options);

		/**
			Copy over an item safely. Checks if item attributes have not been changed before copying an item.

			Shall never fail (always returns false on an error);
	
			todo: May relax the above constrain from external, allow exceptions but cover the exceptions
			inside the caller.
		**/

		void copySafe(IItemContext from, IItemContext to, IRelocationContext context);

		/**
			Try to merge an item of the same type, returns null if merging was not possible.
		**/

		IItem merge(ItemContext source, ItemContext target, IRelocationContext context);
	}
}
