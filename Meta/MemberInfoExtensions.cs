using System.Diagnostics;
using System.Reflection;
using System;

namespace Toolbox.Meta
{
	public static class MemberInfoExtensions
	{
		public static bool hasAttribute<AttributeT>(this MemberInfo mi)
			where AttributeT : Attribute
		{
			return mi.IsDefined(typeof (AttributeT), false);
		}

		public static AttributeT queryAttribute<AttributeT>(this MemberInfo mi)
			where AttributeT : Attribute
		{
			object[] attributes = mi.GetCustomAttributes(typeof(AttributeT), false);

			if (attributes == null || attributes.Length == 0)
				return null;

			if (attributes.Length > 1)
				throw new Exception("multiple attributes of type {0}".format(typeof(AttributeT).Name));

			return (AttributeT)attributes[0];
		}

		public static AttributeT getAttribute<AttributeT>(this MemberInfo mi)
			where AttributeT : Attribute
		{
			var attr = queryAttribute<AttributeT>(mi);
			if (attr == null)
				throw new Exception("Expected attribute of type {0} at method {1}".format(typeof(AttributeT), mi));

			return attr;
		}
	}
}
