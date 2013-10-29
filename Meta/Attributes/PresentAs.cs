using System;

namespace Toolbox.Meta
{
	/**
		Indicates the string representation of an enumeration field.
	**/

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class PresentAsAttribute : Attribute
	{
		public readonly string String;

		public PresentAsAttribute(string name)
		{
			String = name;
		}
	}
}
