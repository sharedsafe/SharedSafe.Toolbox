/**
	An interface that directly represents the state of a replica.
**/

using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	public interface IKnowledge
	{
		/// The root item representing the knowledge.
		IItem RootItem { get; }
	}

	public static class KnowledgeExtensions
	{
		/**
			Create a path tree representing the dirty paths.
		**/

		public static IKnowledgeDiff compare(this IKnowledge a, IKnowledge b, SyncOptions options)
		{
			var collector = new DiffCollector(options);
			DiffAlgorithm.compare(null, a.RootItem, b.RootItem, collector);
			return collector;
		}
	}
}
