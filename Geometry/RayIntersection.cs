/**
	Extensions for Ray to intersect with Planes and Faces
**/

namespace Toolbox.Geometry
{
	public static class RayIntersection
	{
		/**
			@param ray The ray to intersect with the plane.
			@param plane The plane.
			@return The resulting intersection point.

			@note
				This intersection function does intersect the ray with a plane beyond
				the end of the ray, but not a plane before the starting point of the
				ray.
		**/

		public static Vector? intersectWith(this Ray ray, Plane plane)
		{
			if (ray.IsZero)
				return null;

			Vector r0 = ray.Origin;
			Vector rd = ray.Orientation;
			
			double vd = Math.dotProduct(plane.Normal, rd);
			if (vd == 0.0)
				return null; // parallel

			double v0 = -(Math.dotProduct(plane.Normal, r0) + plane.Distance);
			double t = v0 / vd;

			if (t < 0.0)
				return null; // intersection before origin of ray.

			// compute and return intersection point

			return new Vector(r0.X + rd.X * t, r0.Y + rd.Y * t, r0.Z + rd.Z * t);
		}

		/**
			@param ray
				The ray to intersect with the polygon.

			@param begin, end
				The polygon

			@return point
				The intersection point.point.

			Taken from GGEMS 1

			@note
				The previous, 2D intersection function may be used to replace this one.
				It needs to revamped to work for different planes for a set of 3D
				Vector coordinates.
		**/


		public static Vector? intersectWithPolygon(this Ray ray, Vector[] polygon)
		{
			int size = polygon.Length;

			if (size < 3)
				return null;

			Plane plane = Plane.make(polygon);

			Vector? mbP = ray.intersectWith(plane);
			if (mbP == null)
				return null;

			Vector point = mbP.Value;

			// plane intersects with ray

			// find primary indices (we need to project the polygon to one of the
			// primary places, either xy, xz, or yz.

			Pair<int, int> ip = Math.primaryPlanes(plane.Normal);
			int i1 = ip.First;
			int i2 = ip.Second;
			Vector[] v = polygon;

			double u0 = point[i1] - v[0][i1];
			double v0 = point[i2] - v[0][i2];
			bool inter = false;
			uint i = 2;

			do
			{
				/* The polygon is viewed as (n-2) triangles. */
				double u1 = v[i - 1][i1] - v[0][i1];
				double v1 = v[i - 1][i2] - v[0][i2];
				double u2 = v[i][i1] - v[0][i1];
				double v2 = v[i][i2] - v[0][i2];

				if (u1 == 0)
				{
					double beta = u0 / u2;
					if ((beta >= 0.0) && (beta <= 1.0))
					{
						double alpha = (v0 - beta * v2) / v1;
						inter = ((alpha >= 0.0) && ((alpha + beta) <= 1.0));
					}
				}
				else
				{
					double beta = (v0 * u1 - u0 * v1) / (v2 * u1 - u2 * v1);
					if ((beta >= 0.0) && (beta <= 1.0))
					{
						double alpha = (u0 - beta * u2) / u1;
						inter = ((alpha >= 0) && ((alpha + beta) <= 1.0));
					}
				}

			} while ((!inter) && (++i < size));

			if (inter)
				return point;

			return null;
		}
	}
}
