using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox
{
	public static class Indices
	{
		#region Array Indices

		public static IEnumerable<int> indices(this Array a)
		{
			int l = a.Length;
			for (int i = 0; i != l; ++i)
				yield return i;
		}

		public static IEnumerable<int> reverseIndices(this Array a)
		{
			var i = a.Length;
			while (i != 0)
				yield return --i;
		}

		public static IEnumerable<uint> unsignedIndices(this Array a)
		{
			var l = (uint)a.Length;
			for (uint i = 0; i != l; ++i)
				yield return i;
		}

		public static IEnumerable<uint> reverseUnsignedIndices(this Array a)
		{
			var i = (uint)a.Length;
			while (i != 0)
				yield return --i;
		}

		#endregion

		#region List Indices

		public static IEnumerable<int> indices(this IList list)
		{
			int l = list.Count;
			for (int i = 0; i != l; ++i)
				yield return i;
		}

		public static IEnumerable<int> reverseIndices(this IList list)
		{
			int l = list.Count;
			while (l != 0)
				yield return --l;
		}

		#endregion

		#region String Indices

		public static IEnumerable<int> indices(this string str)
		{
			int l = str.Length;
			for (int i = 0; i != l; ++i)
				yield return i;
		}

		public static IEnumerable<int> reverseIndices(this string a)
		{
			var i = a.Length;
			while (i != 0)
				yield return --i;
		}

		#endregion

		public static int[] whereIndices<ElementT>(this ElementT[] array, Func<ElementT, bool> predicate)
		{
			var r = new List<int>();

			for (var i = 0; i != array.Length; ++i)
				if (predicate(array[i]))
					r.Add(i);

			return r.ToArray();
		}

		#region Index Maps

		public static int?[] createIndexMap(this int[] offsets)
		{
			var max = offsets.Max();
			Debug.Assert(max >= 0);

			var res = new int?[max + 1];
			for (var i = 0; i != offsets.Length; ++i)
			{
				var v = offsets[i];
				Debug.Assert(v >= 0);

				res[v] = i;
			}

			return res;
		}

		public static int? indexLookup(this int index, int?[] map)
		{
			Debug.Assert(index >= 0 && index < map.Length);
			return map[index];
		}

		public static TypeT indexLookup<TypeT>(this int index, int?[] map, TypeT[] array)
		{
			Debug.Assert(index >= 0 && index < map.Length);
			return array[map[index].Value];
		}

		#endregion

		#region IEnumerable + Values

		public static IEnumerable<Pair<int, TypeT>> indicesAndValues<TypeT>(this IEnumerable<TypeT> enumerable)
		{
			var i = 0;
			// a LINQ query would add up the indices when the result is evaluated multiple times.
			foreach (var inst in enumerable)
				yield return Pair.make(i++, inst);
		}

		#endregion
	}
}
