using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Collections
{
	public static class DictionaryListExtensions
	{
		public static void add<KeyT, ElementT>(this Dictionary<KeyT, List<ElementT>> dict, KeyT key, ElementT element)
		{
			List<ElementT> list;
			if (!dict.TryGetValue(key, out list))
			{
				list = new List<ElementT>();
				dict.Add(key, list);
			}

			list.Add(element);
		}

		public static bool remove<KeyT, ElementT>(this Dictionary<KeyT, List<ElementT>> dict, KeyT key, ElementT element)
		{
			List<ElementT> list;
			if (!dict.TryGetValue(key, out list))
				return false;

			bool r = list.Remove(element);

			if (list.Count == 0)
				dict.Remove(key);

			return r;
		}

		public static int removeAll<KeyT, ElementT>(this Dictionary<KeyT, List<ElementT>> dict, KeyT key, Predicate<ElementT> elementPredicate)
		{
			List<ElementT> list;
			if (!dict.TryGetValue(key, out list))
				return 0;

			int r = list.RemoveAll(elementPredicate);

			if (list.Count == 0)
				dict.Remove(key);

			return r;
		}

		public static bool contains<KeyT, ElementT>(this Dictionary<KeyT, List<ElementT>> dict, KeyT key, ElementT element)
		{
			List<ElementT> list;
			return dict.TryGetValue(key, out list) && list.Contains(element);
		}

		public static IEnumerable<ElementT> getAll<KeyT, ElementT>(this Dictionary<KeyT, List<ElementT>> dict, KeyT key)
		{
			List<ElementT> list;
			return dict.TryGetValue(key, out list) ? list : Enumerable.Empty<ElementT>();
		}
	}
}
