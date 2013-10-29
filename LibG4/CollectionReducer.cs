#if false

using System;
using System.Diagnostics;

namespace LibG4
{
	/// A symmetric collection reducer.
	/// This stores log n arrays to combine all results in a single one. All intermediate results are
	/// Stored, the number of intermediate results is at maximum the size of the number of elements.

	/// The heap contains always contains n (next power of two) * 2 entries
	/// split up in the following subarrays:
	/// 1 final value
	/// 2 final values
	/// 4 final values
	/// 8 final values (matching up to 16 source entries)
	/// and so on
	/// 
	/// enlarging the array is always done by two and without losing intermediate results

	sealed class CollectionReducer<FromT, ToT> : ICollectionChange<FromT>
	{
		readonly ToT _startValue;
		readonly Func<ToT, ToT, ToT> _combine;

		/// always of _elementCount (next power of two) size
		ToT[] _heap;

		/// The element count
		uint _elementCount;

		/// Update heap, index has been changed!

		/// note: we could reduce the number of heap entries by half, if we would initially combine
		/// the first source values!

		void update(uint index)
		{
			Debug.Assert(index < _elementCount);

			// compute the first combined value slot from the original collection

			// begin of second half table is heap size divided by two.
			uint startTable = _heap.Length >> 1

			// the combined entries offset:
			uint combined = startTable + index >> 1;

			while (combined != 0)
			{
				// the two indices we combine right now!
				uint one = index;
				uint two = index ^ 1;

				_heap[combined] = _combine(_heap[one], _heap[two]);

				index = combined;
				combined >>= 1;
			}

			// entry 1 now contains the final value






			



		}





		#region ICollectionChange<ElementT> Members

		public void insert(uint index, FromT value)
		{
			if (index != _elementCount)
				throw new System.NotImplementedException();

			ensureCapacity(_elementCount);
			rebuild(index);
		}

		public void replace(uint index, FromT value)
		{
			if (index >= _elementCount)
				throw new ArgumentOutOfRangeException("index");

			rebuild(index);
		}

		public void remove(uint index)
		{
			throw new System.NotImplementedException();
		}

		public void clear()
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}

#endif
