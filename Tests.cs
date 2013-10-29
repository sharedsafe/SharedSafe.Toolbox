#if TEST_MWCL && DEBUG && !TOOLBOX_ESSENTIALS

using System;
using NUnit.Framework;

namespace Toolbox.Tests
{ 

	[TestFixture]
	public class MemberwiseClone
	{
		class CloneTest
		{
			public string String;
			public int Value;
			public object Reference;
		};

		const int ObjectCount = 1000000;


		[Test]
		public void test()
		{
			var str = "Hello World";
			var r = new object();

			var sourceArray = new CloneTest[ObjectCount];
			var targetArray = new CloneTest[ObjectCount];
			
			foreach (var i in sourceArray.indices())
				sourceArray[i] = new CloneTest 
				{
					String = str,
					Value = 4223,
					Reference = r
				};

			Func<CloneTest, CloneTest> byHand = ct => new CloneTest
			{
				String = ct.String,
				Value = ct.Value,
				Reference = ct.Reference
			};

			Func<CloneTest, CloneTest> memberwiseClone = ct => ct.memberwiseClone();

			GC.Collect();

			TimeSpan tsByHand = TimeSpan.Zero;

			using (Measure.time(ts => tsByHand = ts))
			{
				clone(sourceArray, targetArray, byHand);
			}

			GC.Collect();
			TimeSpan tsByMwcl = TimeSpan.Zero;

			using (Measure.time(ts => tsByMwcl = ts))
			{
				clone(sourceArray, targetArray, memberwiseClone);
			}

			throw new Exception("hand: " + tsByHand + ", mwcl: " + tsByMwcl);
		}

		void clone(CloneTest[] from, CloneTest[] to, Func<CloneTest, CloneTest> f)
		{
			var l = from.Length;
			for (int i = 0; i != l; ++i)
				to[i] = f(from[i]);
		}
	}
}
#endif
