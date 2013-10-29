using System;

namespace Toolbox.Serialization
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RequiresAssemblyAttribute : Attribute
	{
		// note: the type is not used, but referred, and so the assembly is loaded if the
		// attribute gets instantiated (lazily)
		public RequiresAssemblyAttribute(Type typeInAssembly)
		{
		}
	}
}
