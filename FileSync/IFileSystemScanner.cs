using System.IO;
using SysIO = System.IO;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	public interface IFileSystemScanner
	{
		void scan(IItem dir, IScope scope_, DirectoryInfo dirInfo, bool recursive);
		
		IItem makeFileItem(FileInfo fileInfo);
		IItem makeDirectoryItem(DirectoryInfo directoryInfo);

		FileSystemScannerParam Param { get; }
	}

	public static class FileSystemScannerExtensions
	{
		public static IItem scan(this IFileSystemScanner scanner, IScope scope_, DirectoryInfo info, bool recursive)
		{
			var dir = scanner.makeDirectoryItem(info);
			scanner.scan(dir, scope_, info, recursive);
			return dir;
		}

		public static IItem scanRecursive(this IFileSystemScanner scanner, IScope scope_, DirectoryInfo info)
		{
			return scanner.scan(scope_, info, true);
		}

		public static void scanRecursive(this IFileSystemScanner scanner, IItem folder, IScope scope_, DirectoryInfo info)
		{
			scanner.scan(folder, scope_, info, true);
		}
	}
}