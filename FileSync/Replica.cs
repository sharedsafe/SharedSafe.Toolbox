using SIO = System.IO;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	sealed class Replica : IReplica
	{
		readonly IFileSystem _fs;
		readonly IScanner _scanner;

		public Replica(string rootPath, SyncOptions options, IScanner scanner, IFileSystem fs)
		{
			Path = rootPath;
			Options = options;
			_scanner = scanner;
			_fs = fs;
		}

		#region IReplica Members

		public string Path { get; private set; }
		public SyncOptions Options { get; private set; }

		/**
			Scan and return the current state of the replica.
		**/

		public IKnowledge scan()
		{
			return _scanner.scan(this);
		}

		public IFileSystem FileSystem
		{
			get
			{
				return _fs;
			}
		}

		public string makeAbsolutePath(string relative)
		{
			return SIO.Path.Combine(Path, relative);
		}

		#endregion

		public override string ToString()
		{
			return "File System Replica: {0}".format(Path);
		}
	}
}
