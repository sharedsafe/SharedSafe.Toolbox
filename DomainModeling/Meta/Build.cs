using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DomainModeling.Meta
{
	public sealed class Build
	{
		public static Build<DomainT> forDomainObject<DomainT>()
			where DomainT : IDomainObject
		{
			return new Build<DomainT>();
		}
	}

	public sealed class Build<DomainT>
		where DomainT : IDomainObject
	{
		Type DestructiveEventType_;
		readonly List<Referrer> Referrers = new List<Referrer>();
		Expression<Func<DomainT, Guid>> Id_;

		public Build<DomainT> hasId(Expression<Func<DomainT, Guid>> idAccessor)
		{
			Id_ = idAccessor;
			return this;
		}

		public Build<DomainT> deletedBy<DestructiveT>()
			where DestructiveT : IDestructiveDomainEvent
		{
			DestructiveEventType_ = typeof(DestructiveT);
			return this;
		}

		public Build<DomainT> referredBy<T>(Expression<Func<T, Guid>> expression)
			where T : IDomainObject
		{
			var r = new Referrer<T>(expression);
			Referrers.Add(r);
			return this;
		}

		public static implicit operator MetaType(Build<DomainT> build)
		{
			return build.meta();
		}

		public MetaType meta()
		{
			return new MetaType<DomainT>(DestructiveEventType_, Id_, Referrers.ToArray());
		}
	}
}
