using System.Drawing;
using System.Drawing.Drawing2D;

namespace Toolbox.Drawing
{
	// http://stackoverflow.com/questions/1049328/how-to-create-a-rounded-rectangle-at-runtime-in-windows-forms-with-vb-net-c

	public static class GraphicsExtensions
	{
		public static void drawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rect, float radius)
		{
			var left = rect.Left;
			var top = rect.Top;
			var width = rect.Width;
			var height = rect.Height;

			using (var path = new GraphicsPath())
			{
				path.AddArc(left + width - (radius*2), top, radius*2, radius*2, 270, 90);
				path.AddArc(left + width - (radius*2), top + height - (radius*2), radius*2, radius*2, 0, 90); // Corner
				path.AddArc(left, top + height - (radius*2), radius*2, radius*2, 90, 90);
				path.AddArc(left, top, radius*2, radius*2, 180, 90);
				path.CloseFigure();
				graphics.DrawPath(pen, path);
			}
		}

		public static void drawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF rect, float radius)
		{
			var left = rect.Left;
			var top = rect.Top;
			var width = rect.Width;
			var height = rect.Height;

			using (var path = new GraphicsPath())
			{
				path.AddArc(left + width - (radius * 2), top, radius * 2, radius * 2, 270, 90);
				path.AddArc(left + width - (radius * 2), top + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
				path.AddArc(left, top + height - (radius * 2), radius * 2, radius * 2, 90, 90);
				path.AddArc(left, top, radius * 2, radius * 2, 180, 90);
				path.CloseFigure();
				graphics.DrawPath(pen, path);
			}
		}

		/**
			20101008: new: setting pixel offset mode to highquality pixel aligns the rectangles and exactly fills the given rectangle!
		**/

		public static void fillRoundedRectangle(this Graphics g, Brush b, Rectangle r, float radius)
		{
			var d = radius * 2;
			var mode = g.SmoothingMode;
			var pMode = g.PixelOffsetMode;

			// if we don't anti-alias, pixel garbage results for small pies.
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;

			g.FillPie(b, r.X, r.Y, d, d, 180, 90);
			g.FillPie(b, r.X + r.Width - d, r.Y, d, d, 270, 90);
			g.FillPie(b, r.X, r.Y + r.Height - d, d, d, 90, 90);
			g.FillPie(b, r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);

			g.FillRectangle(b, r.X + radius -.5f, r.Y, r.Width - d + 1.0f, radius);
			g.FillRectangle(b, r.X, r.Y + radius -.5f, r.Width, r.Height - d + 1.0f);
			g.FillRectangle(b, r.X + radius -.5f, r.Y + r.Height - radius, r.Width - d + 1.0f, radius);

			g.SmoothingMode = mode;
			g.PixelOffsetMode = pMode;
		}

		public static void fillRoundedRectangle(this Graphics g, Brush b, RectangleF r, float radius)
		{
			var d = radius * 2;
			var mode = g.SmoothingMode;
			var pMode = g.PixelOffsetMode;

			// if we don't anti-alias, pixel garbage results for small pies.
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;

			g.FillPie(b, r.X, r.Y, d, d, 180, 90);
			g.FillPie(b, r.X + r.Width - d, r.Y, d, d, 270, 90);
			g.FillPie(b, r.X, r.Y + r.Height - d, d, d, 90, 90);
			g.FillPie(b, r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);

			g.FillRectangle(b, r.X + radius - .5f, r.Y, r.Width - d + 1.0f, radius);
			g.FillRectangle(b, r.X, r.Y + radius - .5f, r.Width, r.Height - d + 1.0f);
			g.FillRectangle(b, r.X + radius - .5f, r.Y + r.Height - radius, r.Width - d + 1.0f, radius);

			g.SmoothingMode = mode;
			g.PixelOffsetMode = pMode;
		}
	}
}
