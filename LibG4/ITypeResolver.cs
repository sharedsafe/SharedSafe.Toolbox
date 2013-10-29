using System;

namespace LibG4
{
	public interface ITypeResolver
	{
		Type[] resolve(string name);
	}
}
