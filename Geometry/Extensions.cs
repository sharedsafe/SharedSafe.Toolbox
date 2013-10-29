
namespace Toolbox.Geometry
{
	public static class Extensions
	{
		public static Size toSize(this Point point)
		{
			return new Size(point.X, point.Y);
		}

		public static Point toPoint(this Size size)
		{
			return new Point(size.Width, size.Height);
		}
	}
}
