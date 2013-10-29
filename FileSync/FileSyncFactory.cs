using System;
using SFA = Toolbox.Sync.FileAttributes;
using System.IO;
using Toolbox.Sync;
using FA = System.IO.FileAttributes;

namespace Toolbox.FileSync
{
	public static class FileSyncFactory
	{
		public static IReplica createReplica(string path, SyncOptions options)
		{
			return createReplica(path, options, createScanner());
		}

		public static IReplica createReplica(string path, SyncOptions options, IScanner scanner)
		{
			return new Replica(path, options, scanner, createFileSystem(new NativeFileSystemOptions()));
		}

		public static IReplica createReplica(string path, SyncOptions options, IScanner scanner, IFileSystem fileSystem)
		{
			return new Replica(path, options, scanner, fileSystem);
		}

		public static IScanner createScanner()
		{
			return new Scanner(createFileSystemScanner());
		}


		public static IScanner createScanner(IFileSystemScanner fileSystemScanner)
		{
			return new Scanner(fileSystemScanner);
		}

		public static IFileSystem createFileSystem(NativeFileSystemOptions options)
		{
			return new NativeFileSystem(options);
		}

		#region ItemFilters

		public static Func<FileSystemInfo, bool> DefaultItemFilter
		{
			get
			{
				return info => (0 == (info.Attributes & DefaultIgnoreMask));
			}
		}

		public static Func<FileSystemInfo, bool> chain(this Func<FileSystemInfo, bool> l, Func<FileSystemInfo, bool> r)
		{
			return info => l(info) && r(info);
		}

		public const FA DefaultIgnoreMask =
			// 20100705 We support hidden files, because this is actually a user-setting (and I've seen that svn sets it, too)
			// FA.Hidden |
			FA.System |
			FA.Device |
			FA.Temporary |
			FA.ReparsePoint |
			FA.Offline |
			FA.Encrypted;

		#endregion

		#region Directory Scanning

		public static FileSystemScannerParam createDefaultFileSystemScannerParam()
		{
			return new FileSystemScannerParam
			{
				ItemFilter = DefaultItemFilter,
				CreateFileAttributes = createFileAttributes,
				CreateFolderAttributes = createFolderAttributes
			};
		}

		public static IFileSystemScanner createFileSystemScanner()
		{
			return createFileSystemScanner(createDefaultFileSystemScannerParam());
		}

		public static IFileSystemScanner createFileSystemScanner(FileSystemScannerParam param)
		{
			return new FileSystemScanner(param);
		}

		#endregion


		#region FileInfo / DirectoryInfo => IItem


		public static SFA createFileAttributes(FileInfo fi)
		{
			return new SFA
				{
					Flags = fi.Attributes,
					Length = (ulong)fi.Length,
					LastModificationTime = fi.LastWriteTimeUtc.toExternal()
				};
		}

		public static FolderAttributes createFolderAttributes(DirectoryInfo directoryInfo)
		{
			return new FolderAttributes
			{
				Flags = directoryInfo.Attributes,
				LastModificationTime = directoryInfo.LastWriteTimeUtc.toExternal()
			};
		}

		#endregion
	}
}
