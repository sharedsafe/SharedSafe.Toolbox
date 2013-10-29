namespace Toolbox.Geometry
{
	public struct Vector4
	{
		public Vector4(Vector v)
			: this(v, 1.0)
		{}

		public Vector4(Vector v, double w)
		: this(v.X, v.Y, v.Z, w)
		{}

		public Vector4(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public readonly double X;
		public readonly double Y;
		public readonly double Z;
		public readonly double W;

		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3}", X, Y, Z, W);
		}

		public static implicit operator Vector(Vector4 v)
		{
			if (v.W == 0.0)
				return Vector.Zero;

			return new Vector(v.X, v.Y, v.Z ) / v.W;

		}

		public static readonly Vector4 Zero = new Vector4(0.0, 0.0, 0.0, 0.0);
	}
}
