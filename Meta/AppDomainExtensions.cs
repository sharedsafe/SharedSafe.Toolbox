using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Meta
{
	public static class AppDomainExtensions
	{
		public  static IEnumerable<Type> getTypesWithAttributeSet<AttributeT>(this AppDomain domain)
			where AttributeT : Attribute
		{
			return (from a in domain.GetAssemblies() select a.getTypesWithAttributeSet<AttributeT>()).unfold();
		}

		public static IEnumerable<Pair<Type, AttributeT>> getTypeAttributes<AttributeT>(this AppDomain domain)
			where AttributeT : Attribute
		{
			return (from a in domain.GetAssemblies() select a.getTypeAttributes<AttributeT>()).unfold();
		}
	}
}
