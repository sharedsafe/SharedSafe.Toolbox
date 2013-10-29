using System.Drawing;

namespace Toolbox.Drawing
{
	public static class RectangleExtensions
	{
		public static Rectangle fitInto(this Rectangle rect, Rectangle area)
		{
			if (rect.Right > area.Right)
				rect.Offset(area.Right - rect.Right, 0);
			if (rect.Bottom > area.Bottom)
				rect.Offset(0, area.Bottom - rect.Bottom);

			// left / top clipping has priority

			if (rect.Left < area.Left)
				rect.Offset(area.Left - rect.Left, 0);
			if (rect.Top < area.Top)
				rect.Offset(0, area.Top - rect.Top);

			// then we resize

			if (rect.Width > area.Width)
				rect.Width = area.Width;
			if (rect.Height > area.Height)
				rect.Height = area.Height;

			return rect;
		}
	}
}
