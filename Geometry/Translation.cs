namespace Toolbox.Geometry
{
	/**
		@todo
		Consider renaming Translation to Position

		[or really using the vector type!]
	**/

	public struct Translation : IInterpolatable<Translation>
	{
		public Translation(double x, double y, double z)
			: this(new Vector(x, y, z))
		{ }

		public Translation(Vector v)
		{
			V = v;
		}

		public readonly Vector V;

		public Matrix Matrix
		{
			get
			{
				return Matrix.Identity + V;
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Matrix.Identity - V;
			}
		}

		public override string ToString()
		{
			return V.ToString();
		}

		public static Translation operator - (Translation translation)
		{
			return new Translation(-translation.V);
		}

		public static implicit operator Translation(Vector v)
		{
			return new Translation(v);
		}

		public static readonly Translation Identity = new Translation();

		#region IInterpolatable<Translation> Members

		public Translation interpolate(double coeff, Translation target)
		{
			return V.interpolate(coeff, target.V);
		}

		#endregion
	}
}
