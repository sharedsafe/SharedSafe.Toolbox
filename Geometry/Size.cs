/**
	Size in width/height.
**/

using M = System.Math;

namespace Toolbox.Geometry
{
	public struct Size
	{
		public Size(double width, double height)
		{
			Width = width;
			Height = height;
		}

		public readonly double Width;
		public readonly double Height;


		public static Size operator -(Size l)
		{
			return new Size(-l.Width, -l.Height);
		}

		public Size abs()
		{
			return new Size(M.Abs(Width), M.Abs(Height));
		}

		public override string ToString()
		{
			return Width + "," + Height;
		}
	}
}
