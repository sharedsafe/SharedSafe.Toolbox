/**
	Support to resolve methods. Sometimes required to access internal methods.
**/

using System;
using System.Reflection;

namespace Toolbox.Meta
{
	public static class MethodResolver
	{

		/**
			Resolve a method and bind it to a delegate.

			@param publicTypeInAssembly One arbitrary, public type in that assembly.
			@param typeName Fully qualified type name where the method is in.
			@param methodName The name of the method.
			@param bindingFlags The binding flags to use to look up the method.
				For static methods: BindingFlags.NonPublic | BindingFlags.Static will work.
		**/

		public static DelegateT tryResolve<DelegateT>(Type publicTypeInAssembly, string typeName, string methodName, BindingFlags bindingFlags)
			where DelegateT : class
		{
			var assembly = Assembly.GetAssembly(publicTypeInAssembly);
			if (assembly == null)
				return null;
	
			var type = assembly.GetType(typeName);
			if (type == null)
				return null;

			return Delegate.CreateDelegate(typeof(DelegateT), type, methodName) as DelegateT;
		}


		public static Func<BaseT> tryResolveDefaultConstructor<BaseT>(Type publicTypeInAssembly, string typeName)
		{
			var assembly = Assembly.GetAssembly(publicTypeInAssembly);
			if (assembly == null)
				return null;

			var type = assembly.GetType(typeName);
			if (type == null)
				return null;

			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			if (constructor == null)
				return null;

			return () => (BaseT)constructor.Invoke(null);
		}
	}
}
