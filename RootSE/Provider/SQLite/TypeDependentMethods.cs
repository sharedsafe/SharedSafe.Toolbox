using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RootSE.Provider.SQLite
{
	static class TypeDependentMethods<TypeT>
	{
		public static readonly Func<ColumnQueryParam, TypeT> sqlite3_column = makeSQLite3Column();

		static Func<ColumnQueryParam, TypeT> makeSQLite3Column()
		{
			return param => (TypeT) sqlite3_column_object(param);
		}

		[Obfuscation]
		public static readonly Func<ColumnQueryParam, object> sqlite3_column_object = makeSQLite3ColumnObject();

		static Func<ColumnQueryParam, object> makeSQLite3ColumnObject()
		{
			var t = typeof (TypeT);

			switch (Type.GetTypeCode(t))
			{
				case TypeCode.String:
					return param => (object)Marshal.PtrToStringUni(SQLite.columnText(param.Handle, param.Index));


					// performance warning: (boxing 3 times!)
				case TypeCode.Int32:
					return param => (object)SQLite.columnInt(param.Handle, param.Index);
				case TypeCode.UInt32:
					return param => (object)(uint)SQLite.columnInt(param.Handle, param.Index);

				case TypeCode.Int64:
					return param => (object)SQLite.columnInt64(param.Handle, param.Index);
				case TypeCode.UInt64:
					return param => (object)(ulong)SQLite.columnInt64(param.Handle, param.Index);
		
				case TypeCode.Double:
					return param => (object)SQLite.columnDouble(param.Handle, param.Index);

			}
			if (typeof(Guid) == t)
				return param => ValueEncoder.decodeGuid(Marshal.PtrToStringUni(SQLite.columnText(param.Handle, param.Index)));

			return param => param.Serializer.deserialize(Marshal.PtrToStringUni(SQLite.columnText(param.Handle, param.Index)), t);
		}
	}
}