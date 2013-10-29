using System.IO;
using Toolbox.Sync;

namespace Toolbox.FileSync
{
	/**
		The scanner reads Knowledge from the file system.
	**/
	
	sealed class Scanner : IScanner
	{
		readonly IFileSystemScanner _dirScanner;

		public Scanner(IFileSystemScanner scanner)
		{
			_dirScanner = scanner;
		}

		public IKnowledge scan(IReplica replica)
		{
			var dirInfo = new DirectoryInfo(replica.Path);
			var rootItem = SyncFactory.createRootItem();
			var rootScope = SyncFactory.getRootScope();
			
			_dirScanner.scanRecursive(rootItem, rootScope, dirInfo);

			return SyncFactory.createKnowledge(rootItem);
		}
	}
}
