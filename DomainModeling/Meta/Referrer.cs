using System;
using System.Linq.Expressions;
using Toolbox.Meta;

namespace DomainModeling.Meta
{
	public abstract class Referrer
	{
		public readonly Type Type;
		public readonly string Member;

		protected Referrer(Type type, string member)
		{
			Type = type;
			Member = member;
		}

		public abstract GuidAccessor makeGuidAccessor();
	}

	public sealed class Referrer<DomainT> : Referrer
		where DomainT : IDomainObject
	{
		public readonly Expression<Func<DomainT, Guid>> Accessor;


		public Referrer(Expression<Func<DomainT, Guid>> accessor)
			: base(typeof(DomainT), accessor.nameOfMember())
		{
			Accessor = accessor;
		}

		public override GuidAccessor makeGuidAccessor()
		{
			return new GuidAccessor<DomainT>(Accessor);
		}
	}
}