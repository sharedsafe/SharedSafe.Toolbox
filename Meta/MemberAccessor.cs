using System;
using System.Linq.Expressions;

namespace Toolbox.Meta
{
	/**
		A read/write accessor.
	**/

	public struct MemberAccessor<ContextT, ValueT>
	{
		// these fields are intentionally lowercase so that it looks like calling a method.
		public readonly Func<ContextT, ValueT> get;
		public readonly Action<ContextT, ValueT> set;

		public MemberAccessor(Expression<Func<ContextT, ValueT>> selector)
			: this(selector.Compile(), selector.makeSetter())
		{
		}

		public MemberAccessor(Func<ContextT, ValueT> getter, Action<ContextT, ValueT> setter)
		{
			get = getter;
			set = setter;
		}

		public MemberAccessor<ValueT> resolve(Func<ContextT> context)
		{
			var get = this.get;
			var set = this.set;
			return new MemberAccessor<ValueT>(() => get(context()), v => set(context(), v));
		}
	}

	public struct MemberAccessor<ValueT>
	{
		public readonly Func<ValueT> Get;
		public readonly Action<ValueT> Set;

		public MemberAccessor(Func<ValueT> get, Action<ValueT> set)
		{
			Get = get;
			Set = set;
		}
	}

	public static class MemberAccessorExtensions
	{
		public static MemberAccessor<ContextT, ValueT> toMemberAccessor<ContextT, ValueT>(this Expression<Func<ContextT, ValueT>> sel)
		{
			return new MemberAccessor<ContextT, ValueT>(sel);
		}
	}
}
