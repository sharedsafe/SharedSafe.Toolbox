#if false
using System;
using System.Linq.Expressions;
using Toolbox.Meta;

namespace DomainModeling.Meta
{
	sealed class AttributeAccessor
	{
		readonly Func<object, IAttributes> _getter;
		readonly Action<object, IAttributes> _setter;

		AttributeAccessor(Func<object, IAttributes> getter, Action<object, IAttributes> setter)
		{
			_getter = getter;
			_setter = setter;
		}

		public IAttributes get(object instance)
		{
			return _getter(instance);
		}

		public static AttributeAccessor build<ContextT, ValueT>(Expression<Func<ContextT, ValueT>> accessor)
			where ValueT : IAttributes
		{
			var memberAccessor = accessor.toMemberAccessor();

			Func<object, IAttributes> getter = inst => (IAttributes)memberAccessor.get((ContextT)inst);
			Action<object, IAttributes> setter = (inst, value) => memberAccessor.set((ContextT)inst, (ValueT)value);

			return new AttributeAccessor(getter, setter);
		}
	}
}
#endif
