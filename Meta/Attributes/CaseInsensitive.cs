using System;

namespace Toolbox.Meta
{
	/**
		Indicates that parsing an enumeration shall be case insensitive.
	**/

	[AttributeUsage(AttributeTargets.Enum)]
	public sealed class CaseInsensitiveAttribute : Attribute
	{
	}
}
