
namespace Toolbox.Geometry
{
	public struct Point
	{
		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}

		public readonly double X;
		public readonly double Y;

		public double Length
		{
			get
			{
				return System.Math.Sqrt(X * X + Y * Y);
			}
		}

		public override string ToString()
		{
			return X + "," + Y;
		}

		public static Point operator * (Point l, double s)
		{
			return new Point(l.X * s, l.Y * s);
		}

		public static Point operator / (Point l, double s)
		{
			return new Point(l.X / s, l.Y / s);
		}

		public static Point operator +(Point l, Point r)
		{
			return new Point(l.X + r.X, l.Y + r.Y);
		}


		public static Point operator -(Point l, Point r)
		{
			return new Point(l.X - r.X, l.Y - r.Y);
		}

		public static Point operator -(Point l)
		{
			return new Point(-l.X, -l.Y);
		}

		public static Point operator *(Point l, Point r)
		{
			return new Point(l.X * r.X, l.Y * r.Y);
		}

		public static Point operator /(Point l, Point r)
		{
			return new Point(l.X / r.X, l.Y / r.Y);
		}
	
		#region Point / Size

		public static Point operator +(Point l, Size r)
		{
			return new Point(l.X + r.Width, l.Y + r.Height);
		}

		public static Point operator -(Point l, Size r)
		{
			return l + (-r);
		}

		#endregion

		public Point abs()
		{
			return Math.abs(this);
		}

		public static readonly Point Zero = new Point(0.0, 0.0);

		public bool Equals(Point other)
		{
			return other.X == X && other.Y == Y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Point))
				return false;
			return Equals((Point) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode()*397) ^ Y.GetHashCode();
			}
		}

		public static bool operator ==(Point left, Point right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Point left, Point right)
		{
			return !left.Equals(right);
		}
	}
}
