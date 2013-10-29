using System;

namespace Toolbox.Sync
{
	public interface IFileSystem
	{
		void createFolder(IItemContext prototype, IItemContext item, IRelocationContext context);
		void deleteFolder(IItemContext item, IRelocationContext context);
		void setFolderAttributes(IItemContext item, FolderAttributes newAttributes, IRelocationContext context);
		
		IDisposable tryEnter(IScope scope);

		void createFile(IItemContext prototype, IItemContext item, IRelocationContext context);
		void modifyFile(IItemContext prototype, IItemContext item, IRelocationContext context);
		void deleteFile(IItemContext item, IRelocationContext context);
		void setFileAttributes(IItemContext item, FileAttributes newAttributes, IRelocationContext context);

		/**
			Acquire a stream handle to the content of the item.

			Returns a stream of the content if it matches the path. 

			Never fails:

			Returns null if the item does not match, the path or the
			item has no content (note: contents of zero bytes is content, and shall
			return an acquired stream), or there was some error opening it.
		
			The stream must be disposed after the copy was successful.
		**/

		IAcquiredStream acquireFileStream(IItemContext item, IRelocationContext context);
	}
}
