using System.IO;
using Toolbox.Sync;
using FA = System.IO.FileAttributes;

namespace Toolbox.FileSync
{
	sealed class FileSystemScanner : IFileSystemScanner
	{
		public FileSystemScannerParam Param { get; private set; }

		public FileSystemScanner(FileSystemScannerParam param)
		{
			Param = param;
		}

		public void scan(IItem folder, IScope scope_, DirectoryInfo dirInfo, bool recursive)
		{
			var infos = dirInfo.GetFileSystemInfos();

			foreach (var info in infos)
			{
				if (!Param.ItemFilter(info))
				{
					// todo: this log must be externally accessible by
					// some authority.
					Log.W("Ignored (scan): " + info.Name);
					continue;
				}

				IItem item;

				if ((info.Attributes & FA.Directory) != 0)
				{
					var di = (DirectoryInfo) info;

					item = !recursive 
						? SyncFactory.createFolderItem(info.Name, Param.CreateFolderAttributes(di)) 
						: this.scan(scope_ == null ? null : scope_.enter(info.Name), di, true);
				}
				else
				{
					item = makeFileItem((FileInfo) info);
				}

				folder.addNested(item);
			}
		}

		public IItem makeDirectoryItem(DirectoryInfo info)
		{
			return SyncFactory.createFolderItem(
					info.Name,
					Param.CreateFolderAttributes(info));
		}

		public IItem makeFileItem(FileInfo info)
		{
			return SyncFactory.createFileItem(
				info.Name,
				Param.CreateFileAttributes(info));
		}
	}
}
