using System;
using Toolbox.Testing.LogBased.Detail.Triggers;

namespace Toolbox.Testing.LogBased
{
	public abstract class Trigger
	{
		public abstract IDisposable install(Action triggered);

		#region Combination Operators

		// sequence

		public static Trigger operator & (Trigger l, Trigger r)
		{
			return new SequentialTrigger(l, r);
		}

		// parallel

		public static Trigger operator | (Trigger l, Trigger r)
		{
			return new ParallelTrigger(l, r);
		}

		#endregion
	}
}
