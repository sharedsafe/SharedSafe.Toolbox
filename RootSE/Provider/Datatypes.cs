using System;

namespace RootSE.Provider
{
	static class Datatypes
	{
		public static string toSQL(Type t)
		{
			var tc = Type.GetTypeCode(t);
			switch (tc)
			{
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return "INTEGER";

				case TypeCode.Single:
				case TypeCode.Double:
					return "REAL";

				// everything else is either a string a guid or json encoded.
				default:
					return "TEXT";

			}
		}
	}
}
