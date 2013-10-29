/**
	Reflections can be bound to a particular class. using typeof(ClassName).integrate() a method is looked up
	with two parameters and a return Action (or void) to integrate the type. 

	The method must be tagged with [Integrator] attribute.

	Type based integration seems be a simpler alternative to the regular creation of integrators through the
	IntegrationSpace factory.
**/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Toolbox.Meta;

namespace LibG4
{
	// todo: move this somewhere else.

	public static class TypeExtensions
	{
		public static IDisposable integrate<EnvironmentT>(this Type type, object model, EnvironmentT environment)
		{
			return Detail.IntegratorTypeMap.integrate(type, model, environment);
		}
	}
}


namespace LibG4.Detail
{
	sealed class IntegratorTypeMap
	{
		readonly Dictionary<Type, IntegrationSpace> _map = new Dictionary<Type, IntegrationSpace>();

		public static Integration integrate<EnvironmentT>(Type type, object model, EnvironmentT environment)
		{
			return TypeMap.integrateInternal(type, model, environment);
		}

		Integration integrateInternal<EnvironmentT>(Type type, object model, EnvironmentT environment)
		{
			IntegrationSpace space;
			if (!_map.TryGetValue(type, out space))
			{
				space = createIntegrationSpaceFromType(type);
				_map[type] = space;
			}

			return space.integrate(model, environment);
		}

		static IntegrationSpace createIntegrationSpaceFromType(Type type)
		{
			var methods =
				from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where m.hasAttribute<IntegratorAttribute>()
				select m;

			var space = new IntegrationSpace(type.FullName);

			foreach (var m in methods)
			{
				space.define(createIntegratorFromMethod(m));
			}

			return space;
		}

		static Integrator createIntegratorFromMethod(MethodInfo method)
		{
			checkReflectorMethod(method);

			var returnType = method.ReturnType;

			if (returnType != typeof(void) && returnType != typeof(Action))
				throw new Exception("Return type of integrator method must be either void or Action: " + method);

			var parameters = method.GetParameters();
			Debug.Assert(parameters.Length == 2); // ensured by checkReflectorMethod

			return new Integrator(parameters[0].ParameterType,
				parameters[1].ParameterType, (space, model, env) =>
					{
						var integration = new Integration(space);

						// when we use method integrators, we need to go through the global integration context.

						using (GlobalIntegrationContext.push(integration))
						{
							var r = method.Invoke(null, new[] { model, env });
							var destructor = r as Action;
							if (destructor != null)
								integration.destructor(destructor);
						}

						return integration;
					});
		}

		static void checkReflectorMethod(MethodInfo mi)
		{
			if (mi.IsGenericMethod)
				throw new Exception("Generic Methods can not be used as Reflectors" + mi);
			if (mi.GetParameters().Length != 2)
				throw new Exception("Methods not having 2 parameters can not be used as Reflectors: " + mi);
		}

		static readonly IntegratorTypeMap TypeMap = new IntegratorTypeMap();
	}


}
