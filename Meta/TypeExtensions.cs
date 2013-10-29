using System;
using System.Collections.Generic;

namespace Toolbox.Meta
{
	public static class TypeExtensions
	{
		public static bool isNullable(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition().Equals(NullableType);
		}

		static readonly Type NullableType = typeof (Nullable<>);

		#region Attributes

		public static bool hasAttribute<AttributeT>(this Type t)
		where AttributeT : Attribute
		{
			return t.IsDefined(typeof(AttributeT), false);
		}

		public static AttributeT queryAttribute<AttributeT>(this Type t)
			where AttributeT : Attribute
		{
			object[] attributes = t.GetCustomAttributes(typeof(AttributeT), false);

			if (attributes == null || attributes.Length == 0)
				return null;

			if (attributes.Length > 1)
				throw new Exception("multiple attributes of type {0}".format(typeof(AttributeT).Name));

			return (AttributeT)attributes[0];
		}

		public static AttributeT getAttribute<AttributeT>(this Type t)
			where AttributeT : Attribute
		{
			var attr = queryAttribute<AttributeT>(t);
			if (attr != null)
				return attr;
			
			throw new Exception("missing attribute {0} at type {1}".format(typeof(AttributeT).Name, t.Name));
		}

		#endregion

		public static string present(this Type t)
		{
			var attr = queryAttribute<PresentAsAttribute>(t);
			return attr == null ? t.Name : attr.String;
		}

		/**
			Get all custom Attributes (all means form the type itsself + all inherit classes and from all the interfaces it implements).
		**/

		public static IEnumerable<AttributeT> getCustomAttributesClosure<AttributeT>(this Type t)
			where AttributeT : Attribute
		{
			var aType = typeof(AttributeT);

			foreach (var attr in (AttributeT[])t.GetCustomAttributes(aType, true))
				yield return attr;

			foreach (var iface in t.GetInterfaces())
				foreach (var attr in (AttributeT[])iface.GetCustomAttributes(aType, false))
					yield return attr;
		}
	}
}
