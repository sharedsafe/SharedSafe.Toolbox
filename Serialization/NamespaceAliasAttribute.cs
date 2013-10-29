using System;
using System.Reflection;

namespace Toolbox.Serialization
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class NamespaceAliasAttribute : Attribute
	{
		public readonly string Alias;
		public readonly Type TypeInAssembly_;

		public NamespaceAliasAttribute(string alias)
		{
			Alias = alias;
		}

		public NamespaceAliasAttribute(string alias, Type typeInAssembly)
		{
			Alias = alias;
			TypeInAssembly_ = typeInAssembly;
		}
	}
}