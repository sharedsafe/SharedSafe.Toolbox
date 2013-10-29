using System.Diagnostics;
using S = System;

namespace Toolbox.Geometry
{
	// todo: remove that, this is conflicting with the .NET library

	public static class Math
	{
		public static Vector abs(Vector v)
		{
			return new Vector(S.Math.Abs(v.X), S.Math.Abs(v.Y), S.Math.Abs(v.Z));
		}

		public static Point abs(Point v)
		{
			return new Point(S.Math.Abs(v.X), S.Math.Abs(v.Y));
		}

		public static double dotProduct(Vector a, Vector b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		public static Vector crossProduct(Vector a, Vector b)
		{
			return new Vector(
				a.Y * b.Z - a.Z * b.Y, 
				a.Z * b.X - a.X * b.Z, 
				a.X * b.Y - a.Y * b.X);
		}

		/**
			Computes the normal vector (unnormalized), todo: support complete
			polygons!!!!
		**/

		public static Vector normal(Vector a, Vector b, Vector c)
		{
			return crossProduct(a - b, a - c);
		}

		public static Vector normal(params Vector[] polygon)
		{
			Debug.Assert(polygon.Length >= 3);

			Vector normal = new Vector();
			var nVerts = polygon.Length;

			for (var i = 0; i != nVerts; ++i)
			{
				Vector u = polygon[i];
				Vector v = polygon[i == nVerts - 1 ? 0 : i + 1];

				normal += new Vector(
					(u.Y - v.Y) * (u.Z + v.Z),
					(u.Z - v.Z) * (u.X + v.X),
					(u.X - v.X) * (u.Y + v.Y));
			}

			return normal.Normalized;
		}

		/**
			Return the indices in the range of (0-2, representing x/y/z) of the primary
			plane. The primary plane is the plane perpendicular to the dominant axis of
			the normal vector.
		**/

		public static Pair<int, int> primaryPlanes(Vector normal)
		{
			Vector a = abs(normal);
			if (a.X > a.Y)
				return a.X > a.Z ? Pair.make(1, 2) : Pair.make(0, 1);

			return a.Y > a.Z ? Pair.make(0, 2) : Pair.make(0, 1);
		}

		/// More exact than System.Math.PI

		public const double PI = 3.1415926535897932384626433832795;
	}
}
