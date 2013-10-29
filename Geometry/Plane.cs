/**
	A plane.
**/

namespace Toolbox.Geometry
{
	public struct Plane
	{
		public readonly Vector Normal;
		public readonly double Distance;

		public Plane(Vector normal, double distance)
		{
			Normal = normal;
			Distance = distance;
		}

		/**
			This is a more secure but slower computation of the plane equation, taken
			from Graphic Gems III (Newell's method for computing the plane equation for
			a polygon).
		**/
		
		public static Plane make(Vector[] polygon)
		{
			var normal = Vector.Zero;
			var refPt = Vector.Zero;

			/*
				compute the polygon normal and a reference point on the plane. Note
				that the actual reference point is refpt / nverts

				todo: this is duplicated from math.
			*/

			var nVerts = polygon.Length;

			for (var i = 0; i != nVerts; ++i)
			{
				Vector u = polygon[i];
				Vector v = polygon[i == nVerts-1 ? 0 : i+1];

				normal += new Vector(
					(u.Y - v.Y) * (u.Z + v.Z),
					(u.Z - v.Z) * (u.X + v.X),
					(u.X - v.X) * (u.Y + v.Y));

				refPt += u;
			}

			/*
				normalize the polygon normal to obtain the first three coefficients of
				the plane equation and compute the last coefficient of the plane
				equation
			*/

			var len = normal.Length;

			return new Plane(normal / len, -Math.dotProduct(refPt, normal) / (len * nVerts));
		}
	}

}
