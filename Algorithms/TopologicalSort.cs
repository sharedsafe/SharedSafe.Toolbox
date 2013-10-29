using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Algorithms
{
	public static partial class Algorithms
	{
		// returns roots first.

		public static IEnumerable<NodeT> sortTopologically<NodeT>(this IEnumerable<NodeT> roots, Func<NodeT, IEnumerable<NodeT>> edges)
		{
			return sortTopologicallyReverse(roots, edges).Reverse();
		}

		// returns roots last.

		public static IEnumerable<NodeT> sortTopologicallyReverse<NodeT>(this IEnumerable<NodeT> roots, Func<NodeT, IEnumerable<NodeT>> edges)
		{
			var res = new List<NodeT>();
			var marked = new HashSet<NodeT>();
			foreach (var n in roots)
				addNode(res, marked, n, edges);

			return res;
		}

		static void addNode<NodeT>(ICollection<NodeT> list, HashSet<NodeT> marked, NodeT node, Func<NodeT, IEnumerable<NodeT>> edges)
		{
			if (!marked.Add(node))
				return;

			foreach (var more in edges(node))
				addNode(list, marked, more, edges);

			list.Add(node);
		}
	}
}
