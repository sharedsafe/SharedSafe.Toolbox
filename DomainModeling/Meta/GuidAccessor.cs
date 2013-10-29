using System;
using System.Linq.Expressions;
using Toolbox.Meta;

namespace DomainModeling.Meta
{
	public abstract class GuidAccessor
	{
		public readonly Type Type;

		protected GuidAccessor(Type type)
		{
			Type = type;
		}

		public abstract Guid get(IDomainObject obj);
		public abstract void set(IDomainObject obj, Guid guid);
	}

	sealed class GuidAccessor<DomainT> : GuidAccessor
		where DomainT : IDomainObject
	{
		readonly MemberAccessor<DomainT, Guid> _accessor;

		public GuidAccessor(Expression<Func<DomainT, Guid>> expression)
			: base(typeof(DomainT))
		{
			_accessor = expression.toMemberAccessor();
		}

		public override Guid get(IDomainObject obj)
		{
			return _accessor.get((DomainT) obj);
		}

		public override void set(IDomainObject obj, Guid guid)
		{
			_accessor.set((DomainT) obj, guid);
		}
	}
}
