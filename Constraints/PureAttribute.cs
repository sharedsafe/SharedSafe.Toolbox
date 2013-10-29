using System;

namespace Toolbox.Constraints
{
	/// Indicates that the class is threadsafe (does not maintain state).
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class PureAttribute : Attribute
	{
	}
}
