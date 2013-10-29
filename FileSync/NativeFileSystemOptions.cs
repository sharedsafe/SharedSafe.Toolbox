using System;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	public enum TemporaryNameTag
	{
		None,
		UpcomingDeleteToRecycleBin,
	};

	public struct AcquireStreamParam
	{
		public readonly string Path;
		public readonly FileAttributes Attributes;
		public readonly SyncOptions SyncOptions;

		public AcquireStreamParam(string path, FileAttributes attributes, SyncOptions syncOptions)
		{
			Path = path;
			Attributes = attributes;
			SyncOptions = syncOptions;
		}
	}

	public sealed class NativeFileSystemOptions
	{
		/// This functions returns a new temporary file name for each file or folder
		/// copying or deleting task. 

		/// If this functions is non-null, a file/folder is moved 
		/// first to the temporary file and then renamed to the final one.

		/// Clients should provide different filenames on each call, and file names
		/// that are unlikely to be used otherwise in this directory: For example
		/// {RandomGuid}_appname.tmp

		/// Implementors should try to hide the file from the user and should make
		/// sure that it is deleted on any error or success.

		public Func<TemporaryNameTag, string> UseTemporaryNames_;
		public Func<AcquireStreamParam, IAcquiredStream> AcquireStream = SimpleStreamAcquirer.acquireStream;

		public bool DeleteFilesToRecycleBin;
	}
}
