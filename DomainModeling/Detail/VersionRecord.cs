using System.Reflection;
using RootSE.ORM;

namespace DomainModeling.Detail
{
	[Obfuscation]
	sealed class VersionRecord
	{
		[Unique, Index]
		public string Key;
		public int Version;
	}
}
