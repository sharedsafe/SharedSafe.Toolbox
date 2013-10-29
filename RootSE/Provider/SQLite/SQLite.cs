using System;
using System.Runtime.InteropServices;

namespace RootSE.Provider.SQLite
{
	static class SQLite
	{
		public const int OK = 0;
		public const int ROW = 100;
		public const int DONE = 101;
		public const int INTEGER = 1;
		public const int FLOAT = 2;
		public const int TEXT = 3;
		public const int BLOB = 4;
		public const int NULL = 5;

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_open16", CallingConvention = CallingConvention.Cdecl)]
		public static extern int open(
			[MarshalAs(UnmanagedType.LPWStr)]string filename, out IntPtr db);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
		public static extern int close(IntPtr db);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_prepare16_v2", CallingConvention = CallingConvention.Cdecl)]
		public static extern int prepare(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string zSql,
			int nByte, out IntPtr ppStmpt, IntPtr pzTail);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
		public static extern int step(IntPtr stmHandle);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int finalize(IntPtr stmHandle);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_errmsg16", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr errmsg(IntPtr db);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
		public static extern int columnCount(IntPtr stmHandle);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_origin_name16", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr columnOriginName(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
		public static extern int columnType(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
		public static extern int columnInt(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
		public static extern long columnInt64(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr columnText(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
		public static extern double columnDouble(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_last_insert_rowid", CallingConvention = CallingConvention.Cdecl)]
		public static extern long lastInsertRowId(IntPtr db);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_changes", CallingConvention = CallingConvention.Cdecl)]
		public static extern int changes(IntPtr db);
	}
}
