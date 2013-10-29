#if WPF

using System.Windows;
using System.Windows.Controls;

namespace Toolbox.WPF
{
	public static class CanvasExtensions
	{
		public static void setCanvasPosition(this UIElement element, Point pos)
		{
			Canvas.SetLeft(element, pos.X);
			Canvas.SetTop(element, pos.Y);
		}

		public static Point getCanvasPosition(this UIElement element)
		{
			return new Point(
				Canvas.GetLeft(element),
				Canvas.GetTop(element));
		}
	}
}

#endif