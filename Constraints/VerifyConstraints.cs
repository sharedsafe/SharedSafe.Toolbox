#if false

// project stopped for now, we can not retrieve all assemblies of the current project because of lazy loading.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Toolbox.Constraints
{
	public static class VerifyConstraints
	{
		[Conditional("DEBUG")]

		public static void ofAllAssemblies()
		{
			var app = AppDomain.CurrentDomain;
			var assemblies = app.GetAssemblies();
			var allTypes = assemblies.Select(a => a.GetTypes()).SelectMany(t => t).ToArray();
			Log.D("Verifying constraints of all {0} types in {1} assemblies.".format(allTypes.Length, assemblies.Length));
			foreach (var assembly in assemblies)
			{
				Log.D("assembly: {0}".format(assembly));
				

			}


			verifyPureConstraint(allTypes);
		}

		sealed class PureVerifier
		{
			readonly IEnumerable<Type> _allTypes;
			readonly HashSet<Type> _verified = new HashSet<Type>();
			readonly Dictionary<Type, List<Type>> _implementingTypes = new Dictionary<Type, List<Type>>();

			public PureVerifier(IEnumerable<Type> allTypes)
			{
				_allTypes = allTypes;
			}

			public void verify(IEnumerable<Type> types)
			{
				var queue = new Queue<Type>(types);

				while (queue.Count != 0)
				{
					var current = queue.Dequeue();
					if (_verified.Contains(current))
						continue;

					var dependentTypes = getDependentTypesThatMustBePure(current)
						.Where(t => !isSystemType(t))
						.ToArray();
					
					dependentTypes.forEach(t => verifyMarkedPure(current, t));
					dependentTypes.forEach(queue.Enqueue);

					_verified.Add(current);
					this.D("verified: {0}".format(current));
				}
			}

			IEnumerable<Type> getDependentTypesThatMustBePure(Type t)
			{
				if (t.IsInterface)
					return getAllImplementingTypesOfInterface(t);

				return getAllConstructorParameterTypes(t);
			}

			IEnumerable<Type> getAllConstructorParameterTypes(Type t)
			{
				return t.GetConstructors()
					.Select(c => c.GetParameters())
					.SelectMany(p => p)
					.Select(pi => pi.ParameterType);
			}

			IEnumerable<Type> getAllImplementingTypesOfInterface(Type iface)
			{
				List<Type> l;
				if (!_implementingTypes.TryGetValue(iface, out l))
				{
					l = _allTypes.Where(t => t.GetInterfaces().Any(ifs => ifs.Equals(iface))).ToList();
					_implementingTypes.Add(iface, l);
				}

				return l;
			}

			void verifyMarkedPure(Type referrer, Type t)
			{
				if (!isMarkedPure(t))
					throw new ConstraintVerificationError("Type {0} is required to be marked pure (referred by {1})".format(t, referrer));
			}

			static bool isSystemType(Type t)
			{
				return t.FullName.StartsWith("System.");
			}
		}

		static void verifyPureConstraint(Type[] types)
		{
			var ctx = new PureVerifier(types);
			var typesThatMustBePure = types.Where(isMarkedPure);
			ctx.verify(typesThatMustBePure);
		}

		static bool isMarkedPure(Type t)
		{
			return t.GetCustomAttributes(typeof (PureAttribute), false).Length != 0;
		}
	}
}
#endif
