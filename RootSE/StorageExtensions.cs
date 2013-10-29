using System.Collections.Generic;
using System.Linq;

namespace RootSE
{
	public static class StorageExtensions
	{
		public static IEnumerable<TypeT> queryAll<TypeT>(this IStorage storage)
		{
			return storage.queryAll(typeof (TypeT)).Cast<TypeT>();
		}
	}
}