using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.IoC
{
	sealed class ScopeDictionary<KeyT, ValueT> : IDictionary<KeyT, ValueT>
	{
		readonly ScopeDictionary<KeyT, ValueT> _parent_;
		readonly Dictionary<KeyT, ValueT> _dictionary = new Dictionary<KeyT, ValueT>();

		public ScopeDictionary(ScopeDictionary<KeyT, ValueT> parent_)
		{
			_parent_ = parent_;
		}

		#region IDictionary

		public IEnumerator<KeyValuePair<KeyT, ValueT>> GetEnumerator()
		{
			return enumerateAll().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<KeyT, ValueT> item)
		{
			_dictionary.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<KeyT, ValueT> item)
		{
			return _dictionary.Contains(item) || _parent_ != null && _parent_.Contains(item);
		}

		public void CopyTo(KeyValuePair<KeyT, ValueT>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<KeyT, ValueT> item)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get
			{
				return (_parent_ == null ? 0 : _parent_.Count ) + _dictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool ContainsKey(KeyT key)
		{
			return _dictionary.ContainsKey(key) || _parent_ != null && _parent_.ContainsKey(key);
		}

		public void Add(KeyT key, ValueT value)
		{
			_dictionary.Add(key, value);
		}

		public bool Remove(KeyT key)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(KeyT key, out ValueT value)
		{
			return _dictionary.TryGetValue(key, out value) || _parent_ != null && _parent_.TryGetValue(key, out value);
		}

		public ValueT this[KeyT key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public ICollection<KeyT> Keys
		{
			get { throw new NotImplementedException(); }
		}

		public ICollection<ValueT> Values
		{
			get { throw new NotImplementedException(); }
		}

		#endregion IDictionary

		IEnumerable<KeyValuePair<KeyT, ValueT>> enumerateAll()
		{
			return _parent_ == null ? _dictionary : _dictionary.Concat(_parent_.enumerateAll());
		}
	}
}
