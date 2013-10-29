using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Toolbox.Meta;

/**
	The verification engine is work in progress and may extended when needed.
**/

namespace Toolbox
{
	public abstract class PropertyVerificationAttribute : Attribute
	{
		internal abstract void verify(PropertyInfo property, object value, IVerifier verifier);
	}

	public sealed class Verify : PropertyVerificationAttribute
	{
		public sealed class NotNullOrEmptyAttribute : PropertyVerificationAttribute
		{
			internal override void verify(PropertyInfo property, object value, IVerifier verifier)
			{
				if (value == null)
				{
					verifier.recordError(property, "null");
					return;

				}

				var str = value as string;
				if (str != null && str.Length == 0)
				{
					verifier.recordError(property, "null or empty");
					return;
				}

				// todo: test this!
				var a = value as Array;
				if (a == null || a.Length != 0)
					return;

				verifier.recordError(property, "null or empty");
			}
		}

		public sealed class NotNullAttribute : PropertyVerificationAttribute
		{
			internal override void verify(PropertyInfo property, object value, IVerifier verifier)
			{
				if (value != null)
					return;

				verifier.recordError(property, "null");
			}
		}


		public sealed class PrimaryNameAttribute : Attribute
		{
		}


		


		internal override void verify(PropertyInfo property, object value, IVerifier verifier)
		{
			throw new NotImplementedException();
		}
	}


	public static class VerifyObjectExtensions
	{

		// right now this clashes with some current projects, because the
		// generic version is preferred (which IMHO should not if the type matches).

		// we may be forced to put verification into a separate namespace, Toolbox is too generic.
#if false
		public static void verify<TypeT>(this TypeT instance)
			// may lift this restriction of a class.
			where TypeT : class
		{
			var verifier = new Verifier();
			verify(typeof(TypeT), instance, verifier);
			if (verifier.Errors.Count != 0)
				throw new VerificationException(verifier);
		}

#endif

		static void verify(Type type, object instance, IVerifier verifier)
		{
			// well, passing null here is an internal errror :(
			Debug.Assert(instance != null);

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			string primaryName = null;

			var typeName = type.Name;

			foreach (var property in properties)
			{
				if (property.hasAttribute<Verify.PrimaryNameAttribute>())
				{
					if (primaryName != null)
						throw new Exception("Internal error, more than one primary name defined for type {0}".format(typeName));

					primaryName = property.GetValue(instance, null) as string;
				}

				var toVerify = from a in property.GetCustomAttributes(false) where a is PropertyVerificationAttribute select (PropertyVerificationAttribute)a;

				foreach (var attribute in toVerify)
				{
					attribute.verify(property, property.GetValue(instance, null), verifier);
				}
			}
		}
	}

	interface IVerifier
	{
		void recordError(PropertyInfo property, string description);
	};

	sealed class Verifier : IVerifier
	{
		public readonly List<Pair<PropertyInfo, string>> Errors = new List<Pair<PropertyInfo, string>>();


		#region IVerifier Members

		public void recordError(PropertyInfo property, string description)
		{
			Errors.Add(Pair.make(property, description));
		}

		#endregion
	}

	sealed class VerificationException : Exception
	{
		readonly IVerifier Verifier;

		public VerificationException(IVerifier verifier)
		{
			Verifier = verifier;
		}

	}

}
