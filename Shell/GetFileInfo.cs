using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Toolbox.Shell
{
	[Flags]
	public enum GFI
	{
		/// get icon
		Icon = 0x000000100,
		/// get display name
		DisplayName = 0x000000200,
		/// get type name
		TypeName = 0x000000400,
		/// get attributes
		Attributes = 0x000000800,
		/// get icon location
		IconLocation = 0x000001000,
		/// return exe type
		ExeType = 0x000002000,
		/// get system icon index
		SysIconIndex = 0x000004000,
		/// put a link overlay on icon
		LinkOverlay = 0x000008000,
		/// show icon in selected state
		Selected = 0x000010000,
		/// get only specified attributes
		Attr_Specified = 0x000020000,
		/// get large icon
		LargeIcon = 0x000000000,
		/// get small icon
		SmallIcon = 0x000000001,
		/// get open icon
		OpenIcon = 0x000000002,
		/// get shell size icon
		ShellIconSize = 0x000000004,
		/// pszPath is a pidl
		PIDL = 0x000000008,
		/// use passed dwFileAttribute
		UseFileAttributes = 0x000000010,
		/// apply the appropriate overlays
		AddOverlays = 0x000000020,
		/// Get the index of the overlay in the upper 8 bits of the iIcon
		OverlayIndex = 0x000000040,
	}

	/// Note: this should never be exposed, make this much more "managed", so that returned icons can be freed, for example!

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct FileInfo
	{
		const int MAX_PATH = 260;
		const int MAX_TYPE = 80;

		public IntPtr hIcon;
		public int iIcon;
		public uint dwAttributes;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
		public string szDisplayName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
		public string szTypeName;
	};

	public static class Shell
	{
		public static void GetFileInfo(string path, uint fileAttributes, out FileInfo fileInfo, GFI flags)
		{
			fileInfo = new FileInfo();

			// don't support these two, they smash the return value.
			Debug.Assert((flags & GFI.ExeType) == 0);
			Debug.Assert((flags & GFI.SysIconIndex) == 0);

			IntPtr ret = SHGetFileInfo(path, fileAttributes, ref fileInfo, (uint)Marshal.SizeOf(fileInfo), (uint)flags);
			if (ret.ToInt32() == 0)
				// todo: find out some way to display a fine error!
				throw new Exception("Win32 Error: " + Marshal.GetLastWin32Error());
		}


		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SHGetFileInfo(
			string pszPath,
			uint dwFileAttributes,
			ref FileInfo psfi,
			uint cbFileInfo,
			uint uFlags);
	}
}
