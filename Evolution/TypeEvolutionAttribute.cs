using System;

namespace Toolbox.Evolution
{
	/// Set this at the root serialization type to specify a type name based evolution.
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
	public sealed class TypeEvolutionAttribute : Attribute
	{
		public readonly string OldTypeName;
		public readonly Type NewType;
		public TypeEvolutionAttribute(string oldTypeName, Type newType)
		{
			OldTypeName = oldTypeName;
			NewType = newType;
		}
	}
}
