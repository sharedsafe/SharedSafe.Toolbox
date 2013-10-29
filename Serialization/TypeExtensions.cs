using System;
using JsonExSerializer;
using Toolbox.Evolution;
using Toolbox.Meta;
#if !TOOLBOX_ESSENTIALS
using LibG4;
#endif

namespace Toolbox.Serialization
{
	public static class TypeExtension
	{
		// todo: may cache contexts by type!

		public static SerializationContext serializationContext(this Type t)
		{

			// to load the assembly, it is enough to refer the Attribute 
			// (attributes are instantiated lazily)
			t.queryAttribute<RequiresAssemblyAttribute>();
			
			var context = new SerializationContext();

			foreach (NamespaceAliasAttribute nsa in t.GetCustomAttributes(typeof(NamespaceAliasAttribute), false))
			{
				var type = nsa.TypeInAssembly_ ?? t;
				context.AddNamespaceBinding(type.Namespace, nsa.Alias, type.Assembly.GetName());
			}
			
			// add type evolution.

			foreach (TypeEvolutionAttribute te in t.GetCustomAttributes(typeof(TypeEvolutionAttribute), false))
				context.AddTypeEvolution(te.OldTypeName, te.NewType);

#if !TOOLBOX_ESSENTIALS
			context.AssemblyTypeResolver = G4.resolveImplementationType;
#endif

			return context;
		}

		public static Serializer serializer(this Type t)
		{
			var context = t.serializationContext();
			return new Serializer(t, context);
		}
	}
}
