namespace Toolbox.Sync.Detail
{
	static class IFileSystemExtensions
	{
		/**
			Delete an item safely. Checks if item attributes have not been changed before deleting an item.

			Shall never fail or except (always shall return false on an error)

			todo: May relax the above constrain from external, allow exceptions but cover the exceptions
			inside the caller.
		**/

		public static void delete(this IFileSystem fs, IItemContext item, IRelocationContext context)
		{
			if (item.isFolder())
				fs.deleteFolder(item, context);
			else
				fs.deleteFile(item, context);
		}
	}
}
