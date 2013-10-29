using System;
using System.Runtime.InteropServices;

namespace Toolbox.Shell
{
	// http://www.pinvoke.net/default.aspx/shell32.shellexecuteex

	public sealed class ExecuteEx
	{
		SHELLEXECUTEINFO info;

		const int SW_SHOWNORMAL = 1;

		public ExecuteEx(string file, string arguments)
		{
			info.cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO));
			info.nShow = SW_SHOWNORMAL;
			info.lpFile = file;
			info.lpParameters = arguments;
		}

		public ExecuteEx(string file)
			: this(file, "")
		{
		}

		public ExecuteEx()
			: this("", "")
		{
		}

		#region Wrapper Properties

		const uint SEE_MASK_INVOKEIDLIST = 0xc;

		public bool InvokeIDList 
		{
			get { return isSet(SEE_MASK_INVOKEIDLIST);} 
			set { set(SEE_MASK_INVOKEIDLIST, value);}
		}

		public string Verb
		{
			get { return info.lpVerb; }
			set { info.lpVerb = value; }
		}

		bool isSet(uint mask)
		{
			return (info.fMask & mask) == mask;
		}

		void set(uint mask, bool enable)
		{
			if (enable)
				info.fMask |= mask;
			else
				info.fMask &= ~mask;
		}

		#endregion

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct SHELLEXECUTEINFO
		{
			public int cbSize;
			public uint fMask;
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpVerb;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpFile;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpParameters;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpDirectory;
			public int nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpClass;
			public IntPtr hkeyClass;
			public uint dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}

		// note that paths need to contain the native file separator... '\\'

		public static bool start(string file, string arguments)
		{
			return (new ExecuteEx(file, arguments)).start();
		}

		public bool start()
		{
			// to be sure that info is not GC collected 
			// note: marshalled refs are automatically pinned
			return ShellExecuteEx(ref info);
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
	}
}
