using System;
using System.Reflection;

namespace Toolbox.Meta
{
	public static class ParameterInfoExtensions
	{

		public static bool hasAttribute<AttributeT>(this ParameterInfo t)
		where AttributeT : Attribute
		{
			return t.IsDefined(typeof(AttributeT), false);
		}

		public static AttributeT queryAttribute<AttributeT>(this ParameterInfo t)
			where AttributeT : Attribute
		{
			object[] attributes = t.GetCustomAttributes(typeof(AttributeT), false);

			if (attributes == null || attributes.Length == 0)
				return null;

			if (attributes.Length > 1)
				throw new Exception("multiple attributes of parameter {0}".format(typeof(AttributeT).Name));

			return (AttributeT)attributes[0];
		}

		public static AttributeT getAttribute<AttributeT>(this ParameterInfo t)
			where AttributeT : Attribute
		{
			var attr = queryAttribute<AttributeT>(t);
			if (attr != null)
				return attr;

			throw new Exception("missing attribute {0} at parameter {1}".format(typeof(AttributeT).Name, t.Name));
		}

	}
}
