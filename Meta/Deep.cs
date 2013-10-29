using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace Toolbox.Meta
{
	public static class Deep
	{
		/**
			Deep comparison by the given type (note: not! by the given instance, which may contain a lot
			of stuff we don't want to see (derived types for example).

			Recursion handles public properties of that type, of all the types 
			derived from and all IEnumerable<TypeT> interface contents.

			@note
				By now, this function is not optimized, and may be slow, really slow!
		**/

		public static bool equals<TypeT>(TypeT l, TypeT r)
		{
			return equals(typeof(TypeT), l, r);
		}

		public static bool equals(Type t, object l, object r)
		{
			// reference equality always means deep equality!

			if (ReferenceEquals(l, r))
				return true;

			// one of the two may null!
			if (l == null || r == null)
				return false;
			
			foreach (var field in t.GetFields(BindingFlags.Public))
			{
				var lv = field.GetValue(l);
				var rv = field.GetValue(r);
				if (!equals(field.FieldType, lv, rv))
					return false;
			}
			
			// why do we need properties here if we compare all fields?
			// note: removed that, I see no case where a property is required to be 
			// compared if all fields match.
			// Note: we have no test for this function.

#if false
			foreach (var property in t.GetProperties(BindingFlags.Public))
			{
				var lv = property.GetValue(l, null);
				var rv = property.GetValue(r, null);
				if (!equals(property.PropertyType, lv, rv))
					return false;
			}

#endif

			// each property equals, now check enumerations to be sure.
			// we go for the generic version, to find out the interfaces we look at (not the implementation types, again!)

			foreach (var iface in t.GetInterfaces())
			{
				if (!iface.IsGenericType)
					continue;

				if (!iface.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
					continue;

				// now use the non-generic IEnumerable to iterate using the element type given.
				// note: more than one IEnumerable<> may be used over the IEnumerable non-generic version, we
				// will compare all the variants in the loop, which is fine.

				var elementType = iface.GetGenericArguments()[0];

				var le = ((IEnumerable)l).GetEnumerator();
				var re = ((IEnumerable)r).GetEnumerator();

				while (true)
				{
					var ln = le.MoveNext();
					var rn = re.MoveNext();
					if (ln != rn)
						return false;

					if (!ln)
						break;

					if (!equals(elementType, le.Current, re.Current))
						return false;
				}
			}

			return true;
		}
	}
}
