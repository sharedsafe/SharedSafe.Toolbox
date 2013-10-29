using System.Diagnostics;

namespace Toolbox.Geometry
{
	public struct Vector : IInterpolatable<Vector>
	{
		public Vector(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public readonly double X;
		public readonly double Y;
		public readonly double Z;

		public double Length
		{
			get
			{
				return System.Math.Sqrt(LengthP2);
			}
		}

		// Sometimes this is sufficient for comparing or collecting and saves us a SQRT!

		public double LengthP2
		{
			get { return X*X + Y*Y + Z*Z; }
		}

		public double this[uint idx]
		{
			get
			{
				switch (idx)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2: 
						return Z;
				}

				Debug.Assert(false);
				return 0.0;
			}
		}

		public double this[int idx]
		{
			get { return this[(uint)idx]; }
		}

		public bool IsZero
		{
			/// We don't care about "accidental nulls", this is not epsilon safe, and
			/// we don't want it.

			get 
			{
				return this == Zero; 
			}
		}

		/**
			Normalize a vector (supports input vectors of length 0).
		**/

		public Vector Normalized
		{
			get
			{
				double l = Length;
				return l > 0.0 ? this / l : this;
			}
		}

		public Vector cross(Vector r)
		{
			return Math.crossProduct(this, r);
		}

		#region IInterpolatable<Vector> Members

		public Vector interpolate(double coeff, Vector target)
		{
			return this + (target - this) * coeff;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("{0},{1},{2}", X, Y, Z);
		}

		public static Vector operator * (Vector l, double s)
		{
			return new Vector(l.X * s, l.Y * s, l.Z * s);
		}

		public static Vector operator / (Vector l, double s)
		{
			return new Vector(l.X / s, l.Y / s, l.Z / s);
		}

		public static Vector operator +(Vector l, Vector r)
		{
			return new Vector(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
		}

		public static Vector operator -(Vector l, Vector r)
		{
			return new Vector(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
		}

		public static Vector operator -(Vector l)
		{
			return new Vector(-l.X, -l.Y, -l.Z);
		}

		public static Vector operator *(Vector l, Vector r)
		{
			return new Vector(l.X * r.X, l.Y * r.Y, l.Z * r.Z);
		}

		public static Vector operator /(Vector l, Vector r)
		{
			return new Vector(l.X / r.X, l.Y / r.Y, l.Z / r.Z);
		}

		public static readonly Vector Zero = new Vector(0.0, 0.0, 0.0);

		#region R# Equality

		public bool Equals(Vector other)
		{
			return other.X == X && other.Y == Y && other.Z == Z;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof (Vector))
				return false;
			return Equals((Vector) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = X.GetHashCode();
				result = (result*397) ^ Y.GetHashCode();
				result = (result*397) ^ Z.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Vector left, Vector right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vector left, Vector right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
