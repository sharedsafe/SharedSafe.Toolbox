using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox
{
	public sealed class Chain<ValueT> : IEnumerable<ValueT>
	{
		public readonly Chain<ValueT> Parent_;

		public Chain(ValueT value)
			: this(null, value)
		{
		}

		public Chain(Chain<ValueT> parent_, ValueT value)
		{
			Parent_ = parent_;
			Value = value;
		}

		public ValueT Value { get; private set; }

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<ValueT> GetEnumerator()
		{
			return ValuesReversed.Reverse().GetEnumerator();
		}

		#endregion

		public IEnumerable<ValueT> ValuesReversed
		{
			get
			{
				var current = this;
				do
				{
					yield return current.Value;
					current = current.Parent_;
				} while (current != null);
			}
		}

	}

	public static class ChainExtensions
	{
		public static Chain<ValueT> attach<ValueT>(this Chain<ValueT> chain_, ValueT value)
		{
			return new Chain<ValueT>(chain_, value);
		}
	}
}
