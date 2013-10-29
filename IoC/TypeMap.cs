using System;
using System.Collections.Generic;

namespace Toolbox.IoC
{
	sealed class TypeMap
	{
		readonly TypeMap _parent_;
		readonly Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

		public TypeMap(TypeMap parent_)
		{
			_parent_ = parent_;
		}

		public void add(Type from, Type t)
		{
			_typeMap.Add(from, t);
		}

		Type tryResolve(Type t)
		{
			Type resolved;
			if (_typeMap.TryGetValue(t, out resolved))
				return resolved;

			return _parent_ == null ? null : _parent_.tryResolve(t);
		}

		public Type resolveOrSame(Type t)
		{
			return tryResolve(t) ?? t;
		}
	}
}