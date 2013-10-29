using System;
using System.Collections.Generic;

namespace Toolbox
{
	public static class DictionaryExtensions
	{
		public static ValueT GetOrCreate<KeyT, ValueT>(this Dictionary<KeyT, ValueT> dict, KeyT key)
			where ValueT : new()
		{
			ValueT r;
			if (!dict.TryGetValue(key, out r))
			{
				r = new ValueT();
				dict[key] = r;
			}
			return r;
		}

		// note recommended:
		public static ValueT GetOrCreate<KeyT, ValueT>(this Dictionary<KeyT, ValueT> dict, KeyT key, Func<ValueT> constructor)
		{
			ValueT r;
			if (!dict.TryGetValue(key, out r))
			{
				r = constructor();
				dict[key] = r;
			}
			return r;
		}

	}
}
