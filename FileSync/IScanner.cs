using Toolbox.Sync;

namespace Toolbox.FileSync
{
	public interface IScanner
	{
		IKnowledge scan(IReplica replica);
	}
}
