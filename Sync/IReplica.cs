/**
	Replica represents an existing repository of items.
**/

using System.Diagnostics;

namespace Toolbox.Sync
{
	public interface IReplica
	{
		string Path { get; }
		SyncOptions Options { get; }

		IKnowledge scan();

		/// This is never called (and does not need to be implemented) in virtual synchronization.
		IFileSystem FileSystem { get; }

		/// Create an absolute path from a relative path of this replica.
		string makeAbsolutePath(string relativePath);
	}
}
