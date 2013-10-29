/**
	2D Rectangle.
**/

namespace Toolbox.Geometry
{
	public struct Rectangle
	{
		public Rectangle(double left, double top, double right, double bottom)
			: this(new Point(left, top), new Point(right, bottom))
		{ }

		public Rectangle(Point lt, Point rb)
		{
			LeftTop = lt;
			RightBottom = rb;
		}

		public readonly Point LeftTop;
		public readonly Point RightBottom;

		public double Left
		{ get { return LeftTop.X; } }

		public double Top
		{ get { return LeftTop.Y; } }

		public double Right
		{ get { return RightBottom.X; } }

		public double Bottom
		{ get { return RightBottom.Y; } }

		public double Width
		{ get { return Right - Left; } }

		public double Height
		{ get { return Bottom - Top; } }

		public bool HasContent
		{
			get
			{
				return Bottom > Top && Right > Left;
			}
		}

		/// CCW Polygon starting at left / top

		public Vector[] toPolygon(double z)
		{
			return new[] 
			{
				new Vector(Left, Top, z),
				new Vector(Left, Bottom, z),
				new Vector(Right, Bottom, z),
				new Vector(Right, Top, z)
			};
		}

		public override string ToString()
		{
			return LeftTop + "-" + RightBottom;
		}

#if false
		
		public static bool operator ==(Rectangle l, Rectangle r)
		{
			return l.LeftTop == r.LeftTop && l.RightBottom == r.RightBottom;
		}

		public static bool operator !=(Rectangle l, Rectangle r)
		{
			return !(l == r);
		}
#endif


		public bool Equals(Rectangle other)
		{
			return other.LeftTop.Equals(LeftTop) && other.RightBottom.Equals(RightBottom);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Rectangle))
				return false;
			return Equals((Rectangle) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (LeftTop.GetHashCode()*397) ^ RightBottom.GetHashCode();
			}
		}

		public static bool operator ==(Rectangle left, Rectangle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Rectangle left, Rectangle right)
		{
			return !left.Equals(right);
		}
	}
}
