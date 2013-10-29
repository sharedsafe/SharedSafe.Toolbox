
namespace Toolbox
{
	public static class CharExtensions
	{
		public static string quote(this char c, char q)
		{
			return new string(q, 1) + c + q;
		}

		public static string quote(this char c)
		{
			return c.quote('\'');
		}
	}
}
