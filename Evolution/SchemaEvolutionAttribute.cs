using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Toolbox.Evolution
{
	[AttributeUsage(AttributeTargets.Class| AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class SchemaEvolutionAttribute : Attribute
	{
		readonly Type _evolutionType;

		public SchemaEvolutionAttribute(Type t)
		{
			_evolutionType = t;
		}

		public MemberEvolution? queryMemberEvolution(Type t, string member)
		{
			var me = TypeMemberEvolutions ?? (TypeMemberEvolutions = new Dictionary<Type, Dictionary<string, MemberEvolution>>());
			Dictionary<string, MemberEvolution> members;
			if (!me.TryGetValue(t, out members))
				me[t] = members = makeMemberEvolutionTable(t).ToDictionary(mevo => mevo.Name, mevo => mevo);

			MemberEvolution r;
			return members.TryGetValue(member, out r) ? (MemberEvolution?)r : null;
		}

		IEnumerable<MemberEvolution> makeMemberEvolutionTable(Type containingType)
		{
			return from m in _evolutionType.GetMethods(BindingFlags.Public | BindingFlags.Static) 
						  where 
						  !m.IsGenericMethod &&
						  m.ReturnType == typeof(void) && 
						  m.GetParameters().Length == 2 &&
						  m.GetParameters()[0].ParameterType.IsAssignableFrom(containingType) &&
						  m.IsDefined(typeof(MemberEvolutionAttribute), false)
						  select new MemberEvolution 
						  {
							Name = ((MemberEvolutionAttribute[]) m.GetCustomAttributes(typeof(MemberEvolutionAttribute), false)).Single().MemberName,
							Method = m 
						  };
		}

		[ThreadStatic]	
		static Dictionary<Type, Dictionary<string, MemberEvolution>> TypeMemberEvolutions;
	}
}
