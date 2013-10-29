#if false

// The following may not work when accessed using interfaces, because the ArrayList is cloned / boxed then

using System.Collections.Generic;
using System;

namespace Toolbox
{
	public struct ArrayList<TypeT> : IList<TypeT>
	{
		TypeT[] _data;
		int _count;
		uint _version;
		const int InitialCapacity = 4;

		public ArrayList(uint capacity)
		{
			_data = new TypeT[capacity];
			_count = 0;
			_version = 0;
		}

		#region IList<TypeT> Members

		public int IndexOf(TypeT item)
		{
			if (_data == null)
				return -1;

			return Array.IndexOf(_data, item, 0, _count);
		}

		public void Insert(int index, TypeT item)
		{
			ensureCapacity(_count + 1);

			Array.Copy(_data, index, _data, index + 1, _count - index - 1);
			_data[index] = item;
			++_count;
			++_version;
		}

		public void RemoveAt(int index)
		{
			testIndex(index);

			Array.Copy(_data, index + 1, _data, index, _count - index - 1);
			_data[_count - 1] = default(TypeT);
			--_count;
			++_version;
		}

		public TypeT this[int index]
		{
			get
			{
				testIndex(index);
				return _data[index];
			}
			set
			{
				testIndex(index);
				_data[index] = value;
				++_version;
			}
		}

		#endregion

		#region ICollection<TypeT> Members

		public void Add(TypeT item)
		{
			ensureCapacity(_count + 1);
			_data[_count++] = item;
			++_version;
		}

		public void Clear()
		{
			if (_data != null)
			{
				Array.Clear(_data, 0, _count);
				_count = 0;
				++_version;
			}
		}

		public bool Contains(TypeT item)
		{
			return IndexOf(item) != -1;
		}

		public void CopyTo(TypeT[] array, int arrayIndex)
		{
			if (_data == null)
				return;

			Array.Copy(_data, 0, array, arrayIndex, _count);
		}

		int ICollection<TypeT>.Count
		{
			get { return _count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(TypeT item)
		{
			int i = IndexOf(item);
			if (i == -1)
				return false;
			RemoveAt(i);
			return true;
		}

		#endregion

		#region IEnumerable<TypeT> Members

		public IEnumerator<TypeT> GetEnumerator()
		{
			var version = _version;
			for (int i = 0; i != _count; ++i)
			{
				if (version != _version)
					throw new Exception("Array was changed while in enumeration");

				yield return _data[i];
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Helpers

		void testIndex(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException("index");
		}

		void ensureCapacity(int capacity)
		{
			if (_data == null)
				_data = new TypeT[Math.Max(InitialCapacity, capacity)];

			int newCapacity = _data.Length;
			while (_data.Length < capacity)
			{
				newCapacity *= 2;
			}

			if (newCapacity != _data.Length)
			{
				var newData = new TypeT[newCapacity];
				Array.Copy(_data, newData, _count);
				_data = newData;
			}
		}

		#endregion
	
	}
}

#endif
