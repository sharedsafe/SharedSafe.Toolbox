using System;
using System.Collections.Generic;

namespace Toolbox.Algorithms
{
	public static partial class Algorithms
	{
		public static IEnumerable<TypeT> closure<TypeT>(IEnumerable<TypeT> roots, Func<TypeT, IEnumerable<TypeT>> query)
		{
			var toProcess = new Queue<TypeT>(roots);
			var result = new HashSet<TypeT>();

			while (toProcess.Count != 0)
			{
				var next = toProcess.Dequeue();
				if (!result.Add(next))
					continue;

				foreach (var q in query(next))
					toProcess.Enqueue(q);
			}

			return result;
		}
	}
}
