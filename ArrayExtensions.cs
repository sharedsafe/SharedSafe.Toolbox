using System;
using System.Linq;

namespace Toolbox
{
	public static class Array<ElementT>
	{
		public static readonly ElementT[] Empty = new ElementT[0];
	}

	public static class ArrayExtensions
	{
		public static void clear(this Array a)
		{
			Array.Clear(a, 0, a.Length);
		}

		public static E[] concat<E>(this E[] left, E[] right)
		{
			var n = new E[left.Length + right.Length];
			Array.Copy(left, n, left.Length);
			Array.Copy(right, 0, n, left.Length, right.Length);
			return n;
		}

		public static R[] map<E, R>(this E[] input, Func<E, R> f)
		{
			var r = new R[input.Length];
			for (int i = 0; i != input.Length; ++i)
				r[i] = f(input[i]);

		
			return r;
		}
		
		public static bool isOneOf<T>(this T item, params T[] items)
			where T : struct
		{
			return items.Contains(item);
		}


// testing stuff needs to be put in a separate dll because nunit shouldn't be referenced
#if false
		[TestFixture]
		public class Test
		{
			enum TE
			{
				One,
				Two,
				Three
			}

			enum TE2
			{
				XX
			};
			
			[Test]
			public void testSingle()
			{
				var t = TE.Two;

				Assert.IsTrue(t.isOneOf(TE.Two, TE.Three));
				Assert.IsTrue(t.isOneOf(TE.Two));
				Assert.IsTrue(t.isOneOf(TE.One, TE.Two));
				Assert.IsFalse(t.isOneOf(TE.One));
				Assert.IsFalse(t.isOneOf(TE.One, TE.Three));

				// Assert.IsFalse(t.IsOneOf(TE.One, TE.Three, TE2.XX));
			}
		}
#endif
	}
}
