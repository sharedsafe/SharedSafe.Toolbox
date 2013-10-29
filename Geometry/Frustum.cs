namespace Toolbox.Geometry
{
	public struct Frustum
	{
		public Frustum(
			double left,
			double right,
			double bottom,
			double top,
			double near,
			double far)
		{
			Left = left;
			Right = right;
			Bottom = bottom;
			Top = top;
			Near = near;
			Far = far;
		}

		public readonly double Left;
		public readonly double Right;
		public readonly double Bottom;
		public readonly double Top;
		public readonly double Near;
		public readonly double Far;

		public Matrix Matrix
		{
			get
			{
				double near2 = 2.0 * Near;

				double width = Right - Left;
				double height = Top - Bottom;
				double depth = Far - Near;

				// http://wiki.delphigl.com/index.php/glFrustum
				// http://www.felixgers.de/teaching/jogl/perspectiveProjection.html

				return new Matrix(new[] 
					{
						near2 / width, 0.0, (Right + Left) / width, 0.0,
						0.0, near2 / height, (Top + Bottom) / height, 0.0,
						0.0, 0.0, -(Far + Near) / depth, -(near2*Far) / depth,
						0.0, 0.0, -1.0, 0.0
					});
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				double n2 = 2.0 * Near;

				double fn2 = Far * n2;

				return new Matrix(new[]
				 {
					 (Right - Left) / n2, 0.0, 0.0, (Right + Left) / n2,
					 0.0, (Top - Bottom) / n2, 0.0, (Top + Bottom) / n2,
					 0.0, 0.0, 0.0, -1.0,
					 0.0, 0.0, -(Far - Near) / fn2, (Far + Near) / fn2
				 });
			}
		}
	}
}
