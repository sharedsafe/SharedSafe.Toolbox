namespace Toolbox.Geometry
{
	public struct Rotation
	{
		public Rotation(Vector axis, double angle)
		{
			Axis = axis;
			Angle = angle;
		}

		public readonly Vector Axis;
		public readonly double Angle;

		public override string ToString()
		{
			return Axis + "," + Angle;
		}

		public Matrix Matrix
		{
			get
			{
				double s = System.Math.Sin(Angle);
				double c = System.Math.Cos(Angle);
				double t = 1.0 - c;

				// todo: optimize for vector for size 0 (may return an identity matrix)

				Vector ax = Axis.Normalized;

				double x = ax.X;
				double y = ax.Y;
				double z = ax.Z;

				double xs = x * s;
				double ys = y * s;
				double zs = z * s;

				var r = new Matrix();

				double xt = x * t;
				double yt = y * t;
				double zt = z * t;

				r[0, 0] = x * xt + c;
				r[1, 0] = x * yt - zs;
				r[2, 0] = x * zt + ys;
				r[3, 0] = 0.0;

				r[0, 1] = y * xt + zs;
				r[1, 1] = y * yt + c;
				r[2, 1] = y * zt - xs;
				r[3, 1] = 0.0;

				r[0, 2] = x * zt - ys;
				r[1, 2] = y * zt + xs;
				r[2, 2] = z * zt + c;
				r[3, 2] = 0.0;

				r[0, 3] = 0.0;
				r[1, 3] = 0.0;
				r[2, 3] = 0.0;
				r[3, 3] = 1.0;

				return r;
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Matrix.Transposed;
			}
		}

		public static readonly Rotation Identity = new Rotation();
	}
}
