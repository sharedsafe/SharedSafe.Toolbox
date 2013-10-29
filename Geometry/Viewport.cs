/**
	A geometric view port definition.

	Describes the 2D area of the view port and the perspective parameterization of
	the 3D space it describes.
**/

using System;


namespace Toolbox.Geometry
{
	public struct Viewport
	{
		public Viewport(Rectangle area, double fovy, double near, double far)
			: this(area, fovy, near, far, false)
		{
		}


		public Viewport(Rectangle area, double fovy, double near, double far, bool flipY)
		{
			// content must have area for a viewport. Please remove views
			// having empty areas.
			
			if (!area.HasContent)
				throw new ArgumentException("area has no content", "area");

			Area = area;
			Fovy = fovy;
			Near = near;
			Far = far;
			FlipY = flipY;
		}

		public readonly Rectangle Area;
		public readonly double Fovy;
		public readonly double Near;
		public readonly double Far;
		public readonly bool FlipY;

		public double Aspect
		{
			get
			{
				return Area.Width / Area.Height;
			}
		}

		public Perspective Perspective
		{
			get
			{
				return new Perspective(Fovy, Aspect, Near, Far, FlipY);
			}
		}

		#region Matrix conversion

		public Matrix Matrix
		{
			get
			{
				return Perspective.Matrix;
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Perspective.InverseMatrix;
			}
		}

		#endregion
	}
}
