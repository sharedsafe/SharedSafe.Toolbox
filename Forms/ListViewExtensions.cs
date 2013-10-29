using System.Drawing;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ListViewExtensions
	{
		public static void autosizeLastColumn(this ListView lv)
		{
			var latestSize = new Size();

			lv.Resize += (s, args) =>
			{
				if (lv.Size == latestSize)
					return;

				latestSize = lv.Size;

				if (lv.Columns.Count != 0)
					lv.Columns[lv.Columns.Count - 1].Width = -2;
			};
		}

		public static void showEmptyText(this ListView lv, string text)
		{

			var font = lv.Font;

			SizeF size;

			using (var test = new Bitmap(1, 1))
			{
				using (var g = Graphics.FromImage(test))
				{
					size = g.MeasureString(text, font);
				}
			}

			var offset = new PointF(20, 10);

			var bm = new Bitmap((int)(offset.X + size.Width + 1), (int)(offset.Y + size.Height + 1));
			using (var g = Graphics.FromImage(bm))
			{
				using (var brush = new SolidBrush(SystemColors.ControlDark))
				{
					g.Clear(lv.BackColor);
					g.DrawString(text, font, brush, offset);
				}
			}

			lv.BackgroundImage = bm;
		}

		public static void hideEmptyText(this ListView lv)
		{
			lv.BackgroundImage = null;
		}

	}
}
