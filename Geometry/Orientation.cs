/**
	Orientation, a quaternion.

	An orientation has a rotation and can be created from a Rotation.
**/

#define OPTIMIZE_MATRIX

using System;

namespace Toolbox.Geometry
{
	public struct Orientation
	{
		public readonly Vector V;
		public readonly double S;

		public static readonly Orientation Identity = new Orientation(Vector.Zero, 1.0);

		public Orientation(Rotation rotation)
		{
			double a = rotation.Angle / 2.0;
			V = rotation.Axis.Normalized * System.Math.Sin(a);
			S = System.Math.Cos(a);
		}

		public double Magnitude
		{
			get
			{
				return System.Math.Sqrt(
					V.X * V.X + V.Y * V.Y + V.Z * V.Z + S * S);
			}
		}

		public Orientation(Vector v, double s)
		{
			V = v;
			S = s;
		}

		public Rotation Rotation
		{
			get
			{
				double len = V.Length;
				len = len > 0.0 ? len : 1.0;

				return new Rotation(V / len, 2.0 * System.Math.Acos(S));
			}
		}

		Orientation Normalized
		{
			get
			{
				var mag = Magnitude;
				if (mag < double.Epsilon)
					return this;

				return new Orientation(V / mag, S / mag);
			}
		}

		#region Conversion to Matrix / InverseMatrix

		public Matrix Matrix
		{
			get
			{
				double xw, yw, zw, xx, yy, yz, xy, xz, zz;

				// calculate coefficients
				Vector a = V;

				xx = a.X * a.X;
				xy = a.X * a.Y;
				xz = a.X * a.Z;
				yy = a.Y * a.Y;
				yz = a.Y * a.Z;
				zz = a.Z * a.Z;

				xw = S * a.X;
				yw = S * a.Y;
				zw = S * a.Z;

				// todo: this could be done by directly initializing matrix
				// with an array containing the values

#if !OPTIMIZE_MATRIX

				Matrix m = new Matrix();

				m[0, 0] = 1.0 - 2.0 * (yy + zz);
				m[0, 1] = 2.0 * (xy - zw);
				m[0, 2] = 2.0 * (xz + yw);
				m[0, 3] = 0.0;

				m[1, 0] = 2.0 * (xy + zw);
				m[1, 1] = 1.0 - 2.0 * (xx + zz);
				m[1, 2] = 2.0 * (yz - xw);
				m[1, 3] = 0.0;


				m[2, 0] = 2.0 * (xz - yw);
				m[2, 1] = 2.0 * (yz + xw);
				m[2, 2] = 1.0 - 2.0 * (xx + yy);
				m[2, 3] = 0.0;


				m[3, 0] = 0.0;
				m[3, 1] = 0.0;
				m[3, 2] = 0.0;
				m[3, 3] = 1.0;

				return m;

#else
				Matrix matrix = new Matrix();
				double[] m = matrix.M;

				m[0] = 1.0 - 2.0 * (yy + zz);
				m[4] = 2.0 * (xy - zw);
				m[8] = 2.0 * (xz + yw);
				m[12] = 0.0;

				m[1] = 2.0 * (xy + zw);
				m[5] = 1.0 - 2.0 * (xx + zz);
				m[9] = 2.0 * (yz - xw);
				m[13] = 0.0;


				m[2] = 2.0 * (xz - yw);
				m[6] = 2.0 * (yz + xw);
				m[10] = 1.0 - 2.0 * (xx + yy);
				m[14] = 0.0;


				m[3] = 0.0;
				m[7] = 0.0;
				m[11] = 0.0;
				m[15] = 1.0;

				return matrix;
#endif
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Matrix.Transposed;
			}
		}

		#endregion

		#region Implicit Conversions

		public static implicit operator Orientation (Rotation rotation)
		{
			return new Orientation(rotation);
		}

		#endregion

		#region Multiplication

		public static Orientation operator *(Orientation l, Orientation r)
		{
			double w1 = l.S;
			double w2 = r.S;

			double x1 = l.V.X;
			double y1 = l.V.Y;
			double z1 = l.V.Z;

			double x2 = r.V.X;
			double y2 = r.V.Y;
			double z2 = r.V.Z;

			return new Orientation(new Vector(
				w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2,
				w1 * y2 + y1 * w2 + z1 * x2 - x1 * z2,
				w1 * z2 + z1 * w2 + x1 * y2 - y1 * x2),

				w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2);
		}


		#endregion

		/**
			Create an orienation
			
			Compute an orientation to point from the origin position to the given
			target position, keeping the 'up' vector of the camera parallel to the
			positive y-axis, or, if this is not possible, the positive z-axis.
		**/

		public static Orientation lookAt(double x, double y, double z)
		{
			return lookAt(new Vector(x, y, z));
		}

#if false
		// old version, from the live project (has some problem with huge Y angles)

		public static Orientation lookAt(Vector v)
		{
			Vector ya = new Vector(0.0, 1.0, 0.0);
			double yAngle = System.Math.Atan2(-v.X, -v.Z);

			Orientation yq = new Orientation(new Rotation(ya, yAngle));

			Vector xa = new Vector(1.0, 0.0, 0.0);
			double xAngle = System.Math.Atan2(v.Y, System.Math.Sqrt(v.X * v.Y + v.Z * v.Z));

			Orientation xq = new Orientation(new Rotation(xa, xAngle));
			return xq * yq;


		}

#endif

#if true

		// from http://www.euclideanspace.com/maths/algebra/vectors/lookat/minorlogic.htm

		public static Orientation lookAt(Vector dir)
		{
			Vector up = new Vector(0.0, 1.0, 0.0);

			Vector z = dir.Normalized;
			Vector x = up.cross(z);
			Vector y = z.cross(x);

			double tr = x.X + y.Y + z.Z;
			var o = new Orientation(new Vector(y.Z - z.Y, z.X - x.Z, x.Y - y.X), tr + 1.0);
			return o.Normalized;
		}

#endif

	}
}
