using System;

namespace Toolbox.Serialization
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
	public sealed class TransientAttribute : JsonExSerializer.JsonExIgnoreAttribute
	{
	}
}
