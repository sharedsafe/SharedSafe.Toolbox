using System;
using System.Reflection;
using Toolbox.Meta;

namespace LibG4.Detail
{
	/**
		Describes a property slot of an object.
	**/

	public sealed class Slot
	{
		/// The name of this slot
		public string Name { get; private set; }
	
		/// The arity of this slot.
		public Arity Arity { get; private set; }

		/// The type of objects that can be used at this slot
		public Type Type { get; private set; }


		public static Slot make(PropertyInfo property)
		{
			Type elementType;
			var arity = makeArity(property, out elementType);

			return new Slot
			{
				Name = property.Name,
				Arity = arity,
				Type = elementType
			};
		}

		static Arity makeArity(PropertyInfo property, out Type elementType)
		{
			if (!property.hasAttribute<OptionalAttribute>())
				return makeDefaultArityForType(property.PropertyType, out elementType);

			if (property.PropertyType.IsArray)
				throw new Exception("Property is [Optional], but is array");
			if (property.PropertyType.isNullable())
				throw new Exception("Property is [Optional], but is nullable");
			if (property.PropertyType.IsValueType)
				throw new Exception("Property is [Optional], but is value type, use nullable instead");

			elementType = property.PropertyType;
			return Arity.Optional;
		}

		static Arity makeDefaultArityForType(Type type, out Type elementType)
		{
			if (type.IsArray)
			{
				elementType = type.GetElementType();
				return Arity.ZeroOrMore;
			}

			if (type.isNullable())
			{
				elementType = type.GetGenericArguments()[0];
				return Arity.Optional;
			}

			elementType = type;
			return Arity.One;
		}
	}
}
