using System;

namespace LibG4
{
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class ImplementationAttribute : Attribute
	{
		public readonly Type Scope;

		public ImplementationAttribute(Type scope)
		{
			Scope = scope;
		}
	}
}
