#if WPF
using System.Windows;

namespace Toolbox.WPF
{
	public static class RectExtensions
	{
		public static Point centered(this Rect r)
		{
			return new Point(r.X + r.Width / 2d, r.Y + r.Height / 2d);
		}
	}
}
#endif
