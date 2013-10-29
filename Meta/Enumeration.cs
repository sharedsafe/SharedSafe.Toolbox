using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Meta
{
	/**
		Note: currently does only work on 0 based, contiguous enumerations!
	**/
	
	public static class Enumeration<EnumT>
		where EnumT : struct, IConvertible
	{
		public static EnumT? tryParse(string input)
		{
			int i = Array.IndexOf(
				CaseInsensitive ? StringsUpperCase : Strings, 
				CaseInsensitive ? input.ToUpperInvariant() : input);
			if (i == -1)
				return null;
			return (EnumT)Values.GetValue(i);
		}

		public static string present(EnumT e)
		{
			// todo: mapping shall be using Values!!!
			return Strings[e.ToInt32(null)];
		}

		public static readonly Type Type = typeof(EnumT);
		public static readonly bool CaseInsensitive = Type.hasAttribute<CaseInsensitiveAttribute>();
		public static readonly bool PresentInUpperCase = Type.hasAttribute<PresentInUpperCaseAttribute>();
		public static readonly bool PresentInLowerCase = Type.hasAttribute<PresentInLowerCaseAttribute>();
		public static readonly string[] Strings = makeStrings();
		public static readonly string[] StringsUpperCase = (from s in Strings select s.ToUpperInvariant()).ToArray();
		// note: chnaged from Array to EnumT!
		public static readonly EnumT[] Values = (EnumT[])Enum.GetValues(Type);

		static string[] makeStrings()
		{
			var strings = Enum.GetNames(Type);

			Debug.Assert(!(PresentInUpperCase && PresentInLowerCase));

			if (PresentInUpperCase)
				strings = (from s in strings select s.ToUpperInvariant()).ToArray();

			if (PresentInLowerCase)
				strings = (from s in strings select s.ToLowerInvariant()).ToArray();

			var fields = Type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (var field in fields)
			{
				// note: PresentAs is more specific and overrides the PresentIn attribute.
				var asString = field.queryAttribute<PresentAsAttribute>();
				if (asString == null)
					continue;

				int value = (int)field.GetValue(null);
				Debug.Assert(value < strings.Length);
				strings[value] = asString.String;
			}

			return strings;
		}
		
		public static readonly int CountValues = Enum.GetNames(Type).Length;
	}

	public static class EnumerationExtensions
	{
		public static string present<EnumT>(this EnumT e)
			where EnumT : struct, IConvertible
		{
			return Enumeration<EnumT>.present(e);
		}
	}
}
