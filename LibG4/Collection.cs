using System;
using System.Collections.Generic;
using Toolbox;

namespace LibG4
{
	public sealed class Collection<ElementT> : IList<ElementT>
	{
		uint _count;
		ElementT[] _buf;
		uint _version;

		public const uint InitialCapacity = 4;

		public Collection()
			: this(InitialCapacity)
		{
		}

		public Collection(uint capacity)
		{
			_buf = new ElementT[capacity];
		}

		public ElementT[] Buffer
		{
			get { return _buf; }
		}

		public event Action<Action<ICollectionChange<ElementT>>> OnCollectionChanged;

		#region IList<ElementT> Members

		public int IndexOf(ElementT item)
		{
			return Array.IndexOf(_buf, item, 0, (int)_count);
		}

		public void Insert(int index, ElementT item)
		{
			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException("index");

			ensureCapacity(index.unsigned() + 1);

			if (index != _count)
				Array.Copy(_buf, index, _buf, index + 1, _count - index);

			_buf[index] = item;
			++_count;
			++_version;

			if (OnCollectionChanged != null)
				OnCollectionChanged(i => i.insert(index.unsigned(), item));
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException("index");

			--_count;

			if (index != _count)
				Array.Copy(_buf, index + 1, _buf, index, _count - index);

			_buf[_count] = default(ElementT);
			++_version;

			if (OnCollectionChanged != null)
				OnCollectionChanged(i => i.remove(index.unsigned()));
		}

		public ElementT this[uint index]
		{
			get
			{
				if (index >= _count)
					throw new ArgumentOutOfRangeException("index");

				return _buf[index];
			}
			set
			{
				if (index >= _count)
					throw new ArgumentOutOfRangeException("index");

				_buf[index] = value;
				++_version;

				if (OnCollectionChanged != null)
					OnCollectionChanged(i => i.replace(index, value));
			}
		}

		public ElementT this[int index]
		{
			get { return this[index.unsigned()];  }
			set { this[index.unsigned()] = value; }
		}

		#endregion

		#region ICollection<ElementT> Members

		public void Add(ElementT item)
		{
			Insert(Count, item);
		}

		public void Add(params ElementT[] elements)
		{
			ensureCapacity(_count + (uint)elements.Length);
			foreach (var element in elements)
				Add(element);
		}

		public void Add(IEnumerable<ElementT> elements)
		{
			// todo: may optimize this by calling ensureCapacity on the length (but then
			// we would iterate twice :()!
			foreach (var element in elements)
				Add(element);
		}

		void ensureCapacity(uint capacity)
		{
			if (capacity <= _buf.Length)
				return;

			// note: length of buf may be zero if explicit capacity was set in constructor.
			var newLength = Math.Max(1, _buf.Length.unsigned());

			while (newLength < capacity)
				newLength <<= 1;

			var newBuf = new ElementT[newLength];
			Array.Copy(_buf, 0, newBuf, 0, _count);
			_buf = newBuf;
		}

		public void Clear()
		{
			Array.Clear(_buf, 0, (int)_count);
			_count = 0;
			++_version;

			if (OnCollectionChanged != null)
				OnCollectionChanged(i => i.clear());
		}

		public bool Contains(ElementT item)
		{
			return IndexOf(item) != -1;
		}

		public void CopyTo(ElementT[] array, int arrayIndex)
		{
			Array.Copy(_buf, 0, array, arrayIndex, _count);
		}

		public int Count
		{
			get { return (int)_count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(ElementT item)
		{
			int i = IndexOf(item);
			if (i == -1)
				return false;
			RemoveAt(i);
			return true;
		}

		#endregion

		#region IEnumerable<ElementT> Members

		public IEnumerator<ElementT> GetEnumerator()
		{
			uint ver = _version;
			for (int i = 0; i != _count; ++i)
			{
				if (ver != _version)
					throw new Exception("Collection was modified while in enumerated");

				yield return _buf[i];
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
