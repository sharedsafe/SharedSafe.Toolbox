using System;

namespace Toolbox.Evolution
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited = false)]
	public sealed class MemberEvolutionAttribute : Attribute
	{
		public readonly string MemberName;

		public MemberEvolutionAttribute(string memberName)
		{
			MemberName = memberName;
		}
	}
}
