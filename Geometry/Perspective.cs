
namespace Toolbox.Geometry
{
	public struct Perspective
	{
		public Perspective(double fovy, double aspect, double near, double far)
			: this(fovy, aspect, near, far, false)
		{
		}


		public Perspective(double fovy, double aspect, double near, double far, bool flipY)
		{
			Fovy = fovy;
			Aspect = aspect;
			Near = near;
			Far = far;
			FlipY = flipY;
		}

		/// Field of view in Y
		public readonly double Fovy;
		public readonly double Aspect;
		public readonly double Near;
		public readonly double Far;
		public readonly bool FlipY;

		public Frustum Frustum
		{
			get
			{
			    double yMax = Near * System.Math.Tan(Fovy * Math.PI / 360.0);
				double yMin = -yMax;
				double xMin = yMin * Aspect;
				double xMax = yMax * Aspect;

				if (FlipY)
				{
					yMin = -yMin;
					yMax = -yMax;
				}

				return new Frustum(xMin, xMax, yMin, yMax, Near, Far);
			}
		}

		#region Matrix conversion

		public Matrix Matrix
		{
			get
			{
				return Frustum.Matrix;
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Frustum.InverseMatrix;
			}
		}

		#endregion
	}
}
