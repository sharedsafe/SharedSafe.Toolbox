using System;
using System.Collections.Generic;

namespace Toolbox.Collections
{
	public sealed class Memoizer<FromT, ToT>
	{
		readonly Func<FromT, ToT> _f;
		readonly Dictionary<FromT, ToT> _dict = new Dictionary<FromT, ToT>();

		public Memoizer(Func<FromT, ToT> f)
		{
			_f = f;
		}

		public ToT this[FromT from]
		{
			get
			{
				ToT to;
				if (!_dict.TryGetValue(from, out to))
				{
					to = _f(from);
					_dict.Add(from, to);
				}
				return to;
			}
		}
	}
}
