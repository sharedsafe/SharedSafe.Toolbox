using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Toolbox.Meta
{
	public static class AssemblyExtensions
	{
		public static IEnumerable<Type> getTypesWithAttributeSet<AttributeT>(this Assembly assembly)
			where AttributeT : Attribute
		{
			return from t in assembly.GetTypes() where t.hasAttribute<AttributeT>() select t;
		}

		public static IEnumerable<Pair<Type, AttributeT>> getTypeAttributes<AttributeT>(this Assembly assembly)
			where AttributeT : Attribute
		{
			return from t in assembly.GetTypes() let attr = t.queryAttribute<AttributeT>() where attr != null select Pair.make(t, attr);
		}

		public static IEnumerable<Type> getSubtypesOf<TypeT>(this Assembly assembly)
		{
			return from t in assembly.GetTypes() where t.IsSubclassOf(typeof(TypeT)) select t;
		}
	}
}
