/**
	A ray, an infinite line originating at a base point, 
	described by a Base Point and a orientation vector (unnormalized).
**/

namespace Toolbox.Geometry
{
	public struct Ray
	{
		public readonly Vector Origin;
		public readonly Vector Orientation;

		public Ray(Line line)
			: this(line.Begin, line.Orientation)
		{
		}

		public Ray(Vector origin, Vector orientation)
		{
			Origin = origin;
			Orientation = orientation;
		}

		public bool IsZero
		{
			get
			{
				return Orientation.IsZero;
			}
		}

		public Ray Normalized
		{
			get
			{
				return new Ray(Origin, Orientation.Normalized);
			}
		}

		public override int GetHashCode()
		{
			return Origin.GetHashCode() ^ Orientation.GetHashCode();
		}
	}
}
