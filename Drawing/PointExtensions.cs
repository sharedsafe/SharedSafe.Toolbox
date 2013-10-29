using System.Drawing;

namespace Toolbox.Drawing
{
	public static class PointExtensions
	{
		#region Point

		public static Point sub(this Point p, Point other)
		{
			return new Point(p.X - other.X, p.Y - other.Y);
		}

		public static Point add(this Point p, Point other)
		{
			return new Point(p.X + other.X, p.Y + other.Y);
		}

		#endregion

		#region PointF

		public static PointF sub(this PointF p, PointF other)
		{
			return new PointF(p.X - other.X, p.Y - other.Y);
		}

		public static PointF add(this PointF p, PointF other)
		{
			return new PointF(p.X + other.X, p.Y + other.Y);
		}

		#endregion
	}
}
