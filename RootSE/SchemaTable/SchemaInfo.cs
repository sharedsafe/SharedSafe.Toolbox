#if false
using System.Reflection;
using RootSE.ORM;

namespace RootSE.SchemaTable
{
	[Obfuscation]
	public sealed class SchemaInfo
	{
		[Index, Unique]
		public string TypeName;
		public string JsonSchema;
	}
}
#endif
