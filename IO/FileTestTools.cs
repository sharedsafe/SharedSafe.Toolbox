using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Toolbox.IO
{
	public static class FileTestTools
	{
		public static void createFile(this string path, byte[] content)
		{
			using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
				fs.Write(content, 0, content.Length);
			}
		}

		/**
			Deletes a file:

			Note: file must exist.
		**/

		public static void deleteFile(this string path)
		{
			// note: this is not transaction safe!
			// The only opportunity I see here is to do some kind of move to move the
			// file away and then delete it... ?

			if (!File.Exists(path))
				throw new IOException("File does not exist");

			if (path.isReadOnly())
				path.clearReadOnly();

			File.Delete(path);
		}

		public static void removeFile(this string path)
		{
			File.Delete(path);
		}

		public static void moveFile(this string file, string newName)
		{
			File.Move(file, newName);
		}

		/**
			Create a directory (and all parent paths).
		**/

		public static void createDirectory(this string path)
		{
			Directory.CreateDirectory(path);
		}

		public static void moveDirectory(this string source, string target)
		{
			Directory.Move(source, target);
		}

		/**
			Remove a directory.

			Throws an exception if the directory contains something.
			If the directory does not exist, does nothing.
		**/

		public static void removeDirectory(this string path)
		{
			if (Directory.Exists(path))
				Directory.Delete(path);
		}

		/// Note: the semantics here is inverted (compare to File.Delete())
		/// Directory.Delete() will throw an exception if the directory does not exist.

		public static void deleteDirectory(this string path)
		{
			Directory.Delete(path);
		}

		/**
			Remove the contents (one layer hierarhcy only) and the directory pointed to by path).
		**/

		public static void removeDirectoryAndContents(this string path)
		{
			removeDirectoryAndContents(path, 0);
		}

		public static void removeDirectoryAndContents(this string path, uint maxDepth)
		{
			// note: not transaction safe.
			if (!Directory.Exists(path))
				return;
			// when someone deletes the directory at this point we
			// will throw an (unexpected) exception!
			deleteDirectoryAndContents(path, maxDepth);
		}

		public static void deleteDirectoryAndContents(this string path)
		{
			deleteDirectoryAndContents(path, 0);
		}

		public static void deleteDirectoryAndContents(this string path, uint maxDepth)
		{
			deleteDirectoryContents(path, maxDepth);
			deleteDirectory(path);
		}

		/**
			Create all directories and subdirectories resulting in a directory at path.
		**/

		public static void createDirectoryPath(this string path)
		{
			Directory.CreateDirectory(path);
		}

		/**
			Deletes the contents of a directory. Does not recurse and does not delete the directory itsself. 
		
			If directories are contained, they must be empty, otherwise an exception is thrown.
		**/

		public static void deleteDirectoryContents(this string path)
		{
			deleteDirectoryContents(path, 0);
		}

		/**
			Delete directory contents and specify the maximum recursion depth.

			Depth 0 only deletes the immediate contents of the directory.
		**/

		public static void deleteDirectoryContents(this string path, uint maxDepth)
		{
			var dirInfo = new DirectoryInfo(path);

			var contents = dirInfo.GetFileSystemInfos();
			foreach (var info in contents)
			{
				if (maxDepth != 0)
				{
					var dir = info as DirectoryInfo;
					if (dir != null)
						deleteDirectoryContents(path.appendPath(dir.Name), maxDepth - 1);
				}

				// clear read only flag!
				if ((info.Attributes & FileAttributes.ReadOnly) != 0)
					info.Attributes = info.Attributes & ~FileAttributes.ReadOnly;

				info.Delete();
			}
		}

		public static bool isDirectory(this string path)
		{
			return Directory.Exists(path);
		}

		#region Random Stuff

		/// Create file with random length and contents.
		/// Note: never creates a file of length null (this has to be tested separately)

		public static void createNonZeroLengthRandomFile(this string path)
		{
			createNonZeroLengthRandomFile(path, 65536);
		}

		public static void createNonZeroLengthRandomFile(this string path, ulong maxLength)
		{
			var length = getNonNullRandom(maxLength);
			path.createRandomFile(length);
		}

		public static void createRandomFile(this string path, ulong length)
		{
			var bytes = getRandomBytes(length);

			createFile(path, bytes);
		}


		public static IDisposable encloseFileStealthModification(this string path)
		{
			var fi = new FileInfo(path);

			var accessDate = fi.LastAccessTimeUtc;
			var lastDate = fi.LastWriteTimeUtc;

			return new DisposeAction(() =>
				{
					fi.LastWriteTimeUtc = lastDate;
					fi.LastAccessTimeUtc = accessDate;

					Debug.Assert(fi.LastWriteTimeUtc == lastDate);
					Debug.Assert(fi.LastAccessTimeUtc == accessDate);
					
					// wtf? why is this required?
					// again, be sure!
					Debug.Assert(fi.LastAccessTimeUtc == accessDate);
				}
				);
		}


		public static DateTime getLastWriteTimeUTC(this string path)
		{
			return File.GetLastWriteTimeUtc(path);
		}

		public static void setLastWriteTimeUTC(this string path, DateTime time)
		{
			File.SetLastWriteTimeUtc(path, time);
		}


		public static void appendRandomContent(this string path)
		{
			var length = getNonNullRandom(65536);
			var bytes = getRandomBytes(length);

			var fi = new FileInfo(path);
			var l = fi.Length;

			using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write))
			{
				Debug.Assert(fs.Position == fi.Length);

				fs.Write(bytes, 0, bytes.Length);
			}
		}

		/**
			Modify file contents randomly.

			Implementation overwrites random block inside the file.
		**/

		public static void modifyFileContent(this string path)
		{
			var info = new FileInfo(path);
			var length = (ulong)info.Length;
			Debug.Assert(length != 0);
			ulong lengthToModify = getNonNullRandom(length);
			ulong offset = 0;
			if (lengthToModify != length)
				offset = getNonNullRandom(length - lengthToModify);

			Debug.Assert(offset + lengthToModify <= length);

			byte[] newData = getRandomBytes(lengthToModify);

			Log.D("Modifying file contents: " + path);
			Log.D("offset: {0}, length {1}".format(offset, lengthToModify));

			// self test dates

			var beforeModification = info.LastWriteTimeUtc;

			using (FileStream fs = info.Open(FileMode.Open, FileAccess.Write))
			{
				fs.Seek((long)offset, SeekOrigin.Begin);
				fs.Write(newData, 0, newData.Length);
			}

			var newInfo = new FileInfo(path);

			if (beforeModification == newInfo.LastWriteTimeUtc)
				Log.E("INTERNAL ERROR: file modified, but last write time is the same: " + newInfo.LastWriteTimeUtc.Ticks);
		}

		static readonly Random Random = new Random();

		static ulong getNonNullRandom(ulong max)
		{
			// note: NextDouble() will never return 1.0, so to allow max, we use max + 1, and
			// to compensate for double errors, we use Math.Max to cut.

			return Math.Min(max, (ulong)(Random.NextDouble() * (max + 1)));
		}

		static byte[] getRandomBytes(ulong size)
		{
			var array = new byte[size];
			Random.NextBytes(array);
			return array;
		}


		public static void createTwoDifferentFiles(string upFile, string downFile)
		{
			do
			{
				upFile.createNonZeroLengthRandomFile();
				downFile.createNonZeroLengthRandomFile();
			} while (upFile.hasSameContent(downFile));
		}

		#endregion

		#region Content

		public static byte[] content(this string file)
		{
			return File.ReadAllBytes(file);
		}

		public static void requireContent(this string file, byte[] content)
		{
			try
			{
				var contents = file.content();
				if (contents.Length != content.Length)
					throw new Exception("Content length differs");

				for (int i = 0; i != contents.Length; ++i)
				{
					if (contents[i] != content[i])
						throw new Exception("Content differs beginning at offset {0}".format(i));
				}
			}
			catch (Exception e)
			{
				throw new Exception(file + ": Content compare failed: " + e.Message);
			}
		}

		public static void requireSameContent(this string file, string anotherFile)
		{
			try
			{
				requireSameContent(file.content(), anotherFile.content());
			}
			catch (Exception e)
			{
				throw new Exception("Content comparison of {0} and {1} failed: ".format(file, anotherFile) + e.Message);
			}
		}

		public static void requireSameContent(byte[] left, byte[] right)
		{
			if (left.Length != right.Length)
				throw new Exception("Content length differs");

			var firstBytesDiffering = searchForFirstDifferingOffset(left, right);
			if (firstBytesDiffering != null)
					throw new Exception("Content differs beginning at offset {0}".format(firstBytesDiffering.Value));
		}

		public static bool hasSameContent(this string file, string anotherFile)
		{
			return hasSameContent(file.content(), anotherFile.content());
		}

		static bool hasSameContent(byte[] left, byte[] right)
		{
			if (left.Length != right.Length)
				return false;

			return null == searchForFirstDifferingOffset(left, right);
		}

		static uint? searchForFirstDifferingOffset(byte[] left, byte[] right)
		{
			Debug.Assert(left.Length == right.Length);

			for (int i = 0; i != left.Length; ++i)
			{
				if (left[i] != right[i])
					return i.unsigned();
			}

			return null;
		}

		#endregion

		#region Existence

		public static void requireNotExisting(this string path)
		{
			if (Directory.Exists(path) || File.Exists(path))
				throw new Exception("File or directory exists, but is expected to not: " + path);
		}

		public static void requireDirectoryExisting(this string path)
		{
			if (!Directory.Exists(path))
				throw new Exception("Directory not existing, but is expected to be: " + path);
		}

		#endregion

		#region Attributes

		public static void requireAttributes(this string path, FileAttributes attr)
		{
			FileAttributes attributes = getAttributes(path);

			if ((attributes & attr) != attr)
				throw new Exception("Required attributes not set");
		}


		public static void requireAttributesNot(this string path, FileAttributes attr)
		{
			FileAttributes attributes = getAttributes(path);

			if ((attributes & attr) != 0)
				throw new Exception("Required attributes not cleared");
		}
		
		public static void setAttributes(this string path, FileAttributes attributes)
		{
			File.SetAttributes(path, attributes);
		}

		public static FileAttributes getAttributes(this string path)
		{
			return path.isDirectory() ? new DirectoryInfo(path).Attributes : new FileInfo(path).Attributes;
		}

		#endregion

		#region ReadOnly

		public static bool isReadOnly(this string path)
		{
			return ((path.getAttributes() & FileAttributes.ReadOnly) != 0);
		}

		public static void clearReadOnly(this string path)
		{
			path.setAttributes(path.getAttributes() & ~FileAttributes.ReadOnly);
		}

		public static void setReadonly(this string path)
		{
			path.setAttributes(path.getAttributes() | FileAttributes.ReadOnly);
		}

		#endregion

		#region Hidden
		public static bool isHidden(this string path)
		{
			return ((path.getAttributes() & FileAttributes.Hidden) != 0);
		}

		public static void clearHidden(this string path)
		{
			path.setAttributes(path.getAttributes() & ~FileAttributes.Hidden);
		}

		public static void setHidden(this string path)
		{
			path.setAttributes(path.getAttributes() | FileAttributes.Hidden);
		}

		#endregion

		#region Last Modified Date

		public static void touch(this string path)
		{
			File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
		}

		#endregion

		#region Tree Diff

		public static void requireSameTreeAndContent(string dir1, string dir2, Func<FileSystemInfo, FileSystemInfo, string> compareFileAttributes)
		{
			var p1 = new DirectoryInfo(dir1);
			var p2 = new DirectoryInfo(dir2);

			if (!p1.Exists)
				throw new Exception("{0} does not exist".format(dir1));
			if (!p2.Exists)
				throw new Exception("{1} does not exist".format(dir2));

			var nestedLeft = p1.GetFileSystemInfos().ToDictionary(fsi => fsi.Name);

			foreach (var r in p2.GetFileSystemInfos())
			{
				FileSystemInfo l;
				if (!nestedLeft.TryGetValue(r.Name, out l))
					throw new Exception("{0} does not exist in left tree.".format(r.FullName));

				var explanation = compareFileAttributes(l, r);
				if (explanation != null)
					throw new Exception("{2}: {0}, {1}".format(l.FullName, r.FullName, explanation));

				if ((l.Attributes & FileAttributes.Directory) != 0)
					requireSameTreeAndContent(l.FullName, r.FullName, compareFileAttributes);
				else
					requireSameContent(l.FullName, r.FullName);

				nestedLeft.Remove(l.Name);
			}

			if (nestedLeft.Count != 0)
				throw new Exception("Left directory {0} contains {1} excess entries.".format(p1.FullName, nestedLeft.Count));
		}

		#endregion
	}
}