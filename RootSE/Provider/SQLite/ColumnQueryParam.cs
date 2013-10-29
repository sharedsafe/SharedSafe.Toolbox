using System;

namespace RootSE.Provider.SQLite
{
	internal struct ColumnQueryParam
	{
		public readonly IValueSerializer Serializer;
		public readonly IntPtr Handle;
		public readonly int Index;

		public ColumnQueryParam(IValueSerializer serializer, IntPtr handle, int index)
		{
			Serializer = serializer;
			Handle = handle;
			Index = index;
		}
	}
}