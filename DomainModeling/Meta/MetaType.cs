using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DomainModeling.Meta
{
	public abstract class MetaType
	{
		public readonly Type DomainType;
		public readonly Type DestructiveEventType_;
		public readonly Referrer[] Referrers;

		protected MetaType(
			Type domainType,
			Type destructiveEventType_, 
			Referrer[] referrers
			)
		{
			DomainType = domainType;
			DestructiveEventType_ = destructiveEventType_;
			Referrers = referrers;
		}

		public Reference referenceOf(Guid id)
		{
			return new Reference(DomainType, id);
		}

		public IDestructiveDomainEvent createDestructiveEvent(Guid id)
		{
			var ev = (IDestructiveDomainEvent)Activator.CreateInstance(DestructiveEventType_);
			ev.Id = id;
			return ev;
		}

		public abstract IEnumerable<GuidAccessor> collectGuidAccessors();
	}

	public sealed class MetaType<DomainT> : MetaType
		where DomainT : IDomainObject
	{
		public readonly Expression<Func<DomainT, Guid>> IdAccessor_;
	
		public MetaType(
			Type destructiveEventType_,
			Expression<Func<DomainT, Guid>> idAccessor_, 
			Referrer[] referrers)

			: base(typeof(DomainT), destructiveEventType_, referrers)
		{
			IdAccessor_ = idAccessor_;
		}

		public override IEnumerable<GuidAccessor> collectGuidAccessors()
		{
			if (IdAccessor_ != null)
				yield return new GuidAccessor<DomainT>(IdAccessor_);

			foreach (var referrer in Referrers)
				yield return referrer.makeGuidAccessor();
		}
	}
}