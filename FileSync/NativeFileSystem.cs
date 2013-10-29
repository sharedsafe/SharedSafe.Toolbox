// tbd: we have no hashes, but content indexers actually may write to files 
// (changing reparse points) :(
// So as long we don't use hashes, we disable content indexing for any new file.

#define DONT_CONTENT_INDEX_NEW_FILES
// #define RECYCLE_BIN

using System;
using System.Diagnostics;
using System.IO;
using Toolbox.IO;
using Toolbox.Sync;
using FileAttributes = Toolbox.Sync.FileAttributes;
// note: we need to use VisualBasic, there is no easy way to use SHFileOperation.
#if RECYCLE_BIN
using Microsoft.VisualBasic.FileIO;
#endif

namespace Toolbox.FileSync
{
	sealed class NativeFileSystem : IFileSystem
	{
		readonly NativeFileSystemOptions _options;

		public NativeFileSystem(NativeFileSystemOptions options)
		{
			_options = options;
		}

		#region Folder

		public void createFolder(IItemContext prototype, IItemContext item, IRelocationContext context)
		{
			var dir = item.getRelativeDirectory();
			var path = item.makeAbsolutePath();

			if (Directory.Exists(path))
				throw context.recordError("Directory already exists", item, null);

			string temporaryPath_ = null;

			if (_options.UseTemporaryNames_ != null)
				temporaryPath_ = item.Replica.makeAbsolutePath(Path.Combine(dir, _options.UseTemporaryNames_(TemporaryNameTag.None)));

			var attributes = (FolderAttributes)prototype.Item_.Attributes;

			var prototypePath = temporaryPath_ ?? path;

			internalCreateFolder(item, prototypePath, context, attributes);

			if (temporaryPath_ == null)
				return;

			try
			{
				Directory.Move(temporaryPath_, path);
			}
			catch (Exception e)
			{
				Directory.Delete(temporaryPath_);
				throw context.recordError("Failed to create directory", item, e);
			}
		}

		static void internalCreateFolder(IItemContext item, string path, IRelocationContext context, FolderAttributes attributes)
		{
			DirectoryInfo info;
			try
			{
				info = Directory.CreateDirectory(path);
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to create directory", item, e);
			}

			try
			{
				setFolderAttributesInternal(info, attributes, context);
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to set attributes on newly created directory", item, e);
			}
		}

		public void deleteFolder(IItemContext item, IRelocationContext context)
		{
			// todo: check if deletion is safe!

			var dir = item.getRelativeDirectory();
			var path = item.makeAbsolutePath();
			string temporaryPath_ = null;

			if (_options.UseTemporaryNames_ != null)
				temporaryPath_ = item.Replica.makeAbsolutePath(Path.Combine(dir, _options.UseTemporaryNames_(TemporaryNameTag.None)));

			var prototypePath = temporaryPath_ ?? path;

			if (temporaryPath_ != null)
			{
				try
				{
					Directory.Move(path, temporaryPath_);
				}
				catch (Exception e)
				{
					throw context.recordError("Failed to delete directory", item, e);
				}
			}

			try
			{
				Directory.Delete(prototypePath);
			}
			catch (Exception e)
			{
				if (temporaryPath_ != null)
					Directory.Move(temporaryPath_, path);

				throw context.recordError("Failed to delete directory", item, e);
			}
		}

		#endregion

		#region Folder Attributes

		public void setFolderAttributes(IItemContext item, FolderAttributes newAttributes, IRelocationContext context)
		{
			Debug.Assert(item.Item_.Attributes != null && newAttributes != null);

			try
			{
				// todo: make this transaction safe and test old Flags / Last Modification Time.
				var info = new DirectoryInfo(item.makeAbsolutePath());
				setFolderAttributesInternal(info, newAttributes, context);
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to set attributes", item, e);
			}
		}


		static void setFolderAttributesInternal(DirectoryInfo info, FolderAttributes attr, IRelocationContext context)
		{
			SyncOptions options = context.Options;

			if (options.CompareFlags)
				info.Attributes = attr.Flags;
			else
				preserveAttributes(info, attr.Flags, options);

			if (options.UseFolderLastModificationTime)
				info.LastWriteTimeUtc = attr.LastModificationTime.toUtcDate();
		}


		#endregion

		#region File

		public void createFile(IItemContext prototype, IItemContext item, IRelocationContext context)
		{
			IAcquiredStream stream;
			try
			{
				// todo: optimize: if source FS == our FS, we can copy the file directly!
				stream = prototype.acquireFileStream(context);
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to acquire file", item, e);
			}

			try
			{
				using (stream)
				{
					var prototypeAttributes = (FileAttributes) prototype.Item_.Attributes;
					copyFile(stream, item, prototypeAttributes);
				}
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to copy file", item, e);
			}
		}

		void copyFile(IAcquiredStream stream, IItemContext item, FileAttributes attributes)
		{
			var dir = item.getRelativeDirectory();
			var finalPath = item.makeAbsolutePath();
			string temporaryPath_ = null;

			if (_options.UseTemporaryNames_ != null)
				temporaryPath_ = item.Replica.makeAbsolutePath(Path.Combine(dir, _options.UseTemporaryNames_(TemporaryNameTag.None)));

			// note: if the file already exists, an exception is thrown.

			var targetOfCopyPath = temporaryPath_ ?? finalPath;

			var newFileStream = File.Create(targetOfCopyPath);
			try
			{
				if (temporaryPath_ != null)
					setTemporaryAttributes(targetOfCopyPath);

				using (newFileStream)
				{
					stream.copyTo(newFileStream);
				}

				if (temporaryPath_ == null)
					setFileAttributesAndLastWriteTime(targetOfCopyPath, attributes);
			}
			catch (Exception)
			{
				// be sure the file is deleted if copying fails.
				File.Delete(targetOfCopyPath);
				throw;
			}
				
			if (temporaryPath_ != null)
				moveTemporaryToFinalLocation(temporaryPath_, finalPath, attributes);
		}

		static void setTemporaryAttributes(string targetOfCopyPath)
		{
			var fi = new FileInfo(targetOfCopyPath);
			fi.Attributes |= System.IO.FileAttributes.Temporary | System.IO.FileAttributes.Hidden | System.IO.FileAttributes.NotContentIndexed;
		}

		static void moveTemporaryToFinalLocation(string temporaryPath, string finalPath, FileAttributes attributes)
		{
			try
			{
				// note: this makes our temporary visible to the user but not to the change 
				// notification tracker (because the filename is the same)
				setFileAttributesAndLastWriteTime(temporaryPath, attributes);
				File.Move(temporaryPath, finalPath);
			}
			catch (Exception)
			{
				// be sure temporary file is deleted if move fails.
				File.Delete(temporaryPath);
				throw;
			}
		}

		public void modifyFile(IItemContext prototype, IItemContext item, IRelocationContext context)
		{
			// we simulate a modification by deleting a file and rewriting it.
			// we don't allow to delete readonly files right now if the files actually get modified.
			deleteFile(item, context, true);
			createFile(prototype, item, context);
		}

		public void deleteFile(IItemContext item, IRelocationContext context)
		{
			deleteFile(item, context, false);
		}

		public void deleteFile(IItemContext item, IRelocationContext context, bool throwOnReadOnly)
		{
			// open file exclusive, compare attributes, delete file (mark for deletion) and then close file.

			var tmpFileTag = _options.DeleteFilesToRecycleBin
				? TemporaryNameTag.UpcomingDeleteToRecycleBin
				: TemporaryNameTag.None;

			try
			{
				string itemPath = item.makeAbsolutePath();
				string temporaryPath_ = null;

				// todo: implement check file security attributes also

				if (_options.UseTemporaryNames_ != null)
				{
					var relativeDir = item.getRelativeDirectory();
					temporaryPath_ = item.Replica.makeAbsolutePath(Path.Combine(relativeDir, _options.UseTemporaryNames_(tmpFileTag)));
				}

				var pathToDelete = itemPath;

				if (temporaryPath_ != null)
				{
					Debug.Assert(_options.UseTemporaryNames_ != null);

					this.I("deleteFile: moving file from {0} to {1}".format(itemPath, temporaryPath_));
					File.Move(itemPath, temporaryPath_);
					pathToDelete = temporaryPath_;

					// if we now delete to recycle bin, we just move the file back :) and clear the temporary path.
					// (the obviously redudant rename was just to inform our Directory Scanner to ignore all the changes)

					// bug: the file could be changed while it is at the original place.
					// but we could move the file to some human readable filename (name+_restored)?

					if (_options.DeleteFilesToRecycleBin)
					{
						File.Move(temporaryPath_, itemPath);
						temporaryPath_ = null;
						pathToDelete = itemPath;
					}
				}

				// critical: now user's file may have been moved away!

				try
				{
					Debug.Assert(item.Item_ != null && item.Item_.Attributes is FileAttributes);
					var fa = (FileAttributes)item.Item_.Attributes;

					// check attributes (on temporary, this is actually atomic with the Delete()));
					// also check file date again, file may have been modified in the meantime!

					var fi = new FileInfo(pathToDelete);

					if (!compareFileAttributes(fi, fa, context.Options))
						throw new Exception("Attributes to not match");

					this.I("deleteFile: deleting {0}".format(pathToDelete));

					bool clearedReadOnlyFlag = false;

					// this is all not transaction safe
					if (fi.IsReadOnly)
					{
						if (throwOnReadOnly)
							throw new Exception("Failed to delete a read-only file.");

						fi.IsReadOnly = false;
						clearedReadOnlyFlag = true;
					}

					try
					{
#if RECYCLE_BIN
						if (_options.DeleteFilesToRecycleBin)
						{
							FileSystem.DeleteFile(pathToDelete,
								UIOption.OnlyErrorDialogs,
								RecycleOption.SendToRecycleBin,
								UICancelOption.ThrowException);
						}
						else
#endif
							File.Delete(pathToDelete);
					}
					catch (Exception)
					{
						if (clearedReadOnlyFlag)
							fi.IsReadOnly = false;
						throw;
					}
				}
				catch (Exception)
				{
					if (temporaryPath_ != null)
					{
						this.W("deleteFile: moving file back from {1} to {0}".format(itemPath, temporaryPath_));
						File.Move(temporaryPath_, itemPath);
					}
					throw;
				}
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to delete file", item, e);
			}
		}


		public void setFileAttributes(IItemContext item, FileAttributes newAttributes, IRelocationContext context)
		{
			throw new NotImplementedException();
		}

		public IAcquiredStream acquireFileStream(IItemContext item, IRelocationContext context)
		{
			try
			{
				var path = item.makeAbsolutePath();
				Debug.Assert(item.Item_ != null && item.Item_.Attributes is FileAttributes);
				var attributes = (FileAttributes)item.Item_.Attributes;

				var acquireStream = _options.AcquireStream;
				var syncOptions = context.Options;

				var param = new AcquireStreamParam(path, attributes, syncOptions);
				var acquiredStream = acquireStream(param);

				try
				{
					// be sure attributes match.);
					if (!compareFileAttributes(new FileInfo(path), attributes, context.Options))
						throw new Exception("File attributes mismatch.");

					return acquiredStream;
				}
				catch (Exception)
				{
					acquiredStream.Dispose();
					throw;
				}
			}
			catch (Exception e)
			{
				throw context.recordError("Failed to acquire file stream", item, e);
			}
		}

		#endregion

		#region File Attributes

		static bool compareFileAttributes(FileInfo fi, FileAttributes attr, SyncOptions options)
		{
			if (options.CompareFlags)
			{
				if (fi.Attributes != attr.Flags)
					return false;
			}

			return fi.LastWriteTimeUtc == attr.LastModificationTime.toUtcDate();
		}

		static void setFileAttributesAndLastWriteTime(string path, FileAttributes prototypeAttributes)
		{
			var fi = new FileInfo(path);

			Debug.Assert((ulong)fi.Length == prototypeAttributes.Length);

			var attributes = prototypeAttributes.Flags;
#if DONT_CONTENT_INDEX_NEW_FILES
			attributes |= System.IO.FileAttributes.NotContentIndexed;
#endif

			if ((attributes & System.IO.FileAttributes.ReadOnly) != 0)
			{
				// on readonly, we need to set the last write time first, and then change
				// the attributes afterwards, otherwise we get a unauthorized access violation!

				fi.LastWriteTimeUtc = prototypeAttributes.LastModificationTime.toUtcDate();
				// problem: this may change LastWriteTimeUtc :(
				fi.Attributes = attributes;

				// todo: we need to set up some LastWriteTimeUtc margin ...
			}
			else
			{
				// set attributes first to avoid screwing up the last write time.
				fi.Attributes = attributes;
				fi.LastWriteTimeUtc = prototypeAttributes.LastModificationTime.toUtcDate();
			}
		}

		static void preserveAttributes(FileSystemInfo info, System.IO.FileAttributes flags, SyncOptions options)
		{
			Debug.Assert(!options.CompareFlags);
			var preserveMask = options.PreservedFlagMask;
			if (preserveMask == 0)
				return;

			var current = info.Attributes;
			var newFlags = (current & ~preserveMask) | (flags & preserveMask);
			if (newFlags != current)
				info.Attributes = newFlags;
		}

		#endregion 

		#region IFileSystemScope Members

		public IDisposable tryEnter(IScope scope)
		{
#if DEBUG_SCOPE
			Debug.Assert(scope != null);
			var scopeInfo = scope.ToString();

			this.D("++ " + scopeInfo);
				
			return new DisposeAction(() => this.D("-- " + scopeInfo));
#else
			return null;

#endif
		}

		#endregion
	}
}
