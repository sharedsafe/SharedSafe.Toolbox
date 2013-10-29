using System.Text;

namespace Toolbox
{
	public static class Coding
	{
		#region UTF8 byte[] <-> string

		public static byte[] encodeUTF8(this string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}

		public static string decodeUTF8(this byte[] str)
		{
			return Encoding.UTF8.GetString(str);
		}

		#endregion
	}
}
