using System;
using System.Reflection;

namespace Toolbox.Meta
{
	public static class EntryPoint
	{
		public static Func<string[], int> findMain(Assembly assembly)
		{
			return findMain<string[]>(assembly, "Main");
		}

		public static Func<ArgT, int> findMain<ArgT>(Assembly assembly, string name)
		{
			Func<ArgT, int> mainMethod = null;

			var types = assembly.GetTypes();
			foreach (var type in types)
			{
				var mi = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
				if (mi == null)
					continue;

				var func = convertMethodInfoToMain<ArgT>(mi);
				if (func == null)
					continue;

				if (mainMethod != null)
					throw new Exception("More than one suitable Main entry point was found");

				mainMethod = func;
			}

			return mainMethod;
		}

		static Func<ArgT, int> convertMethodInfoToMain<ArgT>(MethodInfo mi)
		{
			var parameters = mi.GetParameters();
			if (parameters.Length > 1)
				return null;
			
			bool args = false;

			if (parameters.Length == 1)
			{
				if (parameters[0].ParameterType != typeof(ArgT))
					return null;

				args = true;
			}

			var rType = mi.ReturnType;
			if (rType != typeof(int) && rType != typeof(void))
				return null;

			return (arguments) =>
			{
				object[] parms = args ? new object[] { arguments } : null;

				object r = mi.Invoke(null, parms);
				if (r is int)
					return (int)r;

				return 0;
			};
		}
	}
}
