using System.Collections.Generic;

namespace Toolbox.Sync
{
	public interface IKnowledgeDiff
	{
		bool AnyDiffs { get; }
		IEnumerable<string> dump();
	}
}
