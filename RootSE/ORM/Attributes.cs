using System;

namespace RootSE.ORM
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class RowIdAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NullAttribute : Attribute
	{}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IndexAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class UniqueAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class PrimaryAttribute : Attribute
	{
	}
}
