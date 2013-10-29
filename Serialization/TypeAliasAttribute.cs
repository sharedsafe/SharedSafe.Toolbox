using System;
using System.Collections.Generic;
using Toolbox.Meta;

namespace Toolbox.Serialization
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TypeAliasAttribute : Attribute
	{
		public readonly string Alias;

		public TypeAliasAttribute(string alias)
		{
			Alias = alias;
		}

		public static Type tryResolve(string alias)
		{
			Type r;
			TypeAliases.TryGetValue(alias, out r);
			return r;
		}

		static readonly Dictionary<string, Type> TypeAliases = makeTypeAliases();

		static Dictionary<string, Type> makeTypeAliases()
		{
			var dict = new Dictionary<string, Type>();
			var typeAttributes = AppDomain.CurrentDomain.getTypeAttributes<TypeAliasAttribute>();

			foreach (var attr in typeAttributes)
			{
				var type = attr.First;
				var alias = attr.Second.Alias;
				dict.Add(alias, type);
			}

			return dict;
		}
	}
}
