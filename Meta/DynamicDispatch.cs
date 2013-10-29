using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toolbox.Meta
{
	public enum DispatchBehavior
	{
		Required, //< requires that dispatcher is existing.
		Optional //< returns null when dispatcher is not found
	}
	
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class DispatcherAttribute : Attribute
	{
	}

	public static class DynamicDispatchExtensions
	{
		// may lock this to avoid creating scopes for each thread?
		[ThreadStatic]
		static Dictionary<Type, DDTypeScope> TypeScopes;

		/**
			Dynamic dispatch for instance methods.
		**/

		public static object dispatch(this object scopeInstance, object input, DispatchBehavior behavior = DispatchBehavior.Required)
		{
			var ts = TypeScopes ?? (TypeScopes = new Dictionary<Type, DDTypeScope>());
			var scopeType = (scopeInstance is Type) ? (Type)scopeInstance : scopeInstance.GetType();

			DDTypeScope scope;
			if (!ts.TryGetValue(scopeType, out scope))
				ts[scopeType] = scope = new DDTypeScope(scopeType);

			return scope.dispatch(scopeInstance, input, behavior);
		}

		public static object dispatchTo(this object input, object scopeInstance, DispatchBehavior behavior = DispatchBehavior.Required)
		{
			return scopeInstance.dispatch(input, behavior);
		}

		public static object tryDispatchTo(this object input, object scopeInstance)
		{
			return input.dispatchTo(scopeInstance, DispatchBehavior.Optional);
		}
	}

	sealed class DDTypeScope
	{
		readonly Type _scope;
		readonly Dictionary<Type, MethodInfo> _methods;

		public DDTypeScope(Type scope)
		{
			_scope = scope;

			var methods = from m in scope.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
						  where
						  m.IsDefined(typeof(DispatcherAttribute), false) &&
						  m.GetParameters().Length == 1
						  let pt = m.GetParameters()[0].ParameterType
						  // be sure it's the type definition of the generic parameter!
						  let ptg = pt.IsGenericType && !pt.IsGenericTypeDefinition ? pt.GetGenericTypeDefinition() : pt
						  select new { ParameterType = ptg, M = m };

			_methods = methods.ToDictionary(t => t.ParameterType, t => t.M);

		}

		public object dispatch(object instance, object input, DispatchBehavior behavior)
		{
			var it = (instance as Type) ?? instance.GetType();

			var inputType = input.GetType();

			MethodInfo method;
			if (!_methods.TryGetValue(inputType, out method))
			{
				// generic?

				if (!it.IsGenericType || it.IsGenericTypeDefinition)
					return fail(it, input, behavior);

				var typeDefinition = it.GetGenericTypeDefinition();
				if (!_methods.TryGetValue(typeDefinition, out method))
					return fail(it, input, behavior);

				// create the non-generic variant and cache it for later use
				method = method.MakeGenericMethod(it.GetGenericArguments());
				_methods[inputType] = method;
			}

			// todo: create delegate to get faster!
			return method.Invoke(instance is Type ? null : instance, new[] { input });
		}

		object fail(Type type, object input, DispatchBehavior behavior)
		{
			if (behavior == DispatchBehavior.Optional)
				return null;
			throw new InternalError("Can not dynamically dispatch type {0} in scope {1}".format(type.Name, _scope.Name), input);
		}
	}
}
