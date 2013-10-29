using System.Collections.Generic;
using System;
using System.Linq;

namespace Toolbox
{
	public static class EnumerableExtensions
	{
		public static void forEach<ElementT>(this IEnumerable<ElementT> e, Action<ElementT> action)
		{
			foreach (var i in e)
				action(i);
		}

		/// Returns all true elements in Result.First, all false elements in Result.Second

		public static Pair<IEnumerable<ElementT>, IEnumerable<ElementT>> split<ElementT>(this IEnumerable<ElementT> e, Func<ElementT, bool> predicate)
		{
			var bools = from i in e select predicate(i);
			return Pair.make(mkTrue(e, bools), mkFalse(e, bools));
		}

		static IEnumerable<ElementT> mkFalse<ElementT>(IEnumerable<ElementT> e, IEnumerable<bool> boolEnum)
		{
			using (var bools = boolEnum.GetEnumerator())
				foreach (var i in e)
				{
					bools.MoveNext();
					if (!bools.Current)
						yield return i;
				}
		}

		static IEnumerable<ElementT> mkTrue<ElementT>(IEnumerable<ElementT> e, IEnumerable<bool> boolEnum)
		{
			using (var bools = boolEnum.GetEnumerator())
				foreach (var i in e)
				{
					bools.MoveNext();
					if (bools.Current)
						yield return i;
				}
		}

		#region firstOr

		public static ElementT firstOr<ElementT>(this IEnumerable<ElementT> e, ElementT element)
		{
			using (var enumerator = e.GetEnumerator())
				return !enumerator.MoveNext() ? element : enumerator.Current;
		}

		#endregion

		#region Prepend / Append

		public static IEnumerable<ElementT> prepend<ElementT>(this IEnumerable<ElementT> e, ElementT value)
		{
			yield return value;

			foreach (var element in e)
				yield return element;
		}

		public static IEnumerable<ElementT> append<ElementT>(this IEnumerable<ElementT> e, ElementT value)
		{
			foreach (var element in e)
				yield return element;

			yield return value;
		}

		#endregion

		#region Unfold (SelectMany alternative?)

		public static IEnumerable<A> unfold<A>(this IEnumerable<IEnumerable<A>> folded)
		{
			foreach (var x in folded)
				foreach (var y in x)
					yield return y;
		}

		public static A[] unfold<A>(this A[][] array)
		{
			// fast version for arrays

			int i = 0;
			foreach (var e in array)
				i += e.Length;

			var r = new A[i];
			i = 0;
			foreach (var e in array)
			{
				e.CopyTo(r, i);
				i += e.Length;
			}
			return r;
		}

		#endregion

		#region convert an IEnumerable<A> to an IDisposable instance, where the first entry is evaluated and all others upon dispose.

		public static IDisposable toDisposable<T>(this IEnumerable<T> f)
		{
			using (var enumerator = f.GetEnumerator())
			{
				bool hasNext = enumerator.MoveNext();

				return new DisposeAction(() =>
					{
						while (hasNext)
							hasNext = enumerator.MoveNext();
					});
			}
		}
		#endregion


		#region Shuffle

		public static IEnumerable<T> shuffle<T>(this IEnumerable<T> source)
		{
			return shuffle(source, new Random());
		}

		public static IEnumerable<T> shuffle<T>(this IEnumerable<T> source, Random random)
		{
			T[] array = source.ToArray();

			for (int i = 0; i < array.Length; i++)
			{
				int r = random.Next(i + 1);
				T tmp = array[r];
				array[r] = array[i];
				array[i] = tmp;
			}

			return array;
		}

		#endregion

		#region Side-Effect

		public static IEnumerable<T> act<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T elem in source)
			{
				action(elem);
				yield return elem;
			}
		}

		#endregion

		#region Chunk
		
		public static IEnumerable<IEnumerable<T>> chunk<T>(this IEnumerable<T> source, uint maxLength)
		{
			if (maxLength == 0)
				throw new ArgumentOutOfRangeException("maxLength", "chunk length can not be 0");

			var current = new List<T>();

			foreach (var s in source)
			{
				current.Add(s);
				if (current.Count != maxLength)
					continue;
				yield return current;
				current = new List<T>();
			}


			if (current.Count != 0)
				yield return current;
		}

		#endregion

		#region pair

		public static IEnumerable<Two<T>> pair<T>(this IEnumerable<T> source)
		{
			using (var it = source.GetEnumerator())
			{
				for (; ; )
				{
					if (!it.MoveNext())
						yield break;

					var first = it.Current;

					if (!it.MoveNext())
						yield break;

					var second = it.Current;

					yield return Two.make(first, second);
				}
			}
		}

		#endregion

		#region Enumerator

		public static ValueT next<ValueT>(this IEnumerator<ValueT> enumerator)
		{
			if (!enumerator.MoveNext())
				throw new InternalError("next() failed, end of enumeration");

			return enumerator.Current;
		}

		#endregion
	}
}
