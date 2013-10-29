using System.Collections.Generic;

namespace Toolbox.Algorithms
{
	public static partial class Algorithms
	{
		// Returns two enumerables, the first one contains the elements to be removed from the left list 
		// and the second one contains the elements to be added.

		public static Two<IEnumerable<ElementT>> setDiff<ElementT>(this IEnumerable<ElementT> left, IEnumerable<ElementT> right)
		{
			var allLeft = new HashSet<ElementT>(left);
			var newOnes = new List<ElementT>();

			foreach (var r in right)
			{
				if (!allLeft.Contains(r))
					newOnes.Add(r);
			}

			allLeft.ExceptWith(right);

			return Two.make<IEnumerable<ElementT>>(
				allLeft,
				newOnes
				);
		}
	}
}
