using System;
using System.IO;
using Toolbox.Sync;
using FileAttributes = Toolbox.Sync.FileAttributes;

namespace Toolbox.FileSync
{
	public struct FileSystemScannerParam
	{
		// After construction, contains the default item filter, use "chain()" 
		// extension method and assignment to add a new filter.

		public Func<FileSystemInfo, bool> ItemFilter { get; set; }

		// Constructors to create appropritate attributes.

		public Func<FileInfo, FileAttributes> CreateFileAttributes { get; set; }
		public Func<DirectoryInfo, FolderAttributes> CreateFolderAttributes { get; set; }
	}
}