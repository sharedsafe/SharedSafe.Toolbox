using System;
using System.Reflection;

namespace Toolbox.Evolution
{
	public struct MemberEvolution
	{
		internal string Name;
		internal MethodInfo Method;
		
		public Type ValueType
		{
			get { return Method.GetParameters()[1].ParameterType; }
		}

		public void setValue(object container, object value)
		{
			Method.Invoke(null, new [] { container, value });
		}
	};
}