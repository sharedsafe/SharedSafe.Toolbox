using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Toolbox.Drawing
{
	public static class ImageExtensions
	{
		public static Image rescaleNice(this Image image, Size sz)
		{
			var sizedBitmap = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb);

			using (var graphics = Graphics.FromImage(sizedBitmap))
			{
				// for even better quality, see here: 
				// http://www.nayyeri.net/very-high-quality-image-resizing-in-net

				graphics.Clear(Color.Transparent);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(image, 0, 0, sz.Width, sz.Height);
			}

			return sizedBitmap;
		}

		/// f = 1.0: white f = 0 original image

		public static Image lighten(this Image image, double f)
		{
			return image.overlay(Color.Transparent.mix(Color.White, f));
		}

		public static Image overlay(this Image image, Color color)
		{
			var newBitmap = new Bitmap(image);
			var sz = newBitmap.Size;

			using (var graphics = Graphics.FromImage(newBitmap))
			{
				using (var brush = new SolidBrush(color))
				{
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.FillRectangle(brush, new Rectangle(new Point(), sz));
				}
			}

			return newBitmap;
		}
	}
}
