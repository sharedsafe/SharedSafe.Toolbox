namespace Toolbox.Geometry
{
	/**
		Axis aligned box.
	**/

	public struct Box
	{
		public readonly Vector LeftBottomBack;
		public readonly Vector RightTopFront;

		public Box(Vector lbb, Vector rtf)
		{
			LeftBottomBack = lbb;
			RightTopFront = rtf;
		}

		public double Left
		{
			get
			{
				return LeftBottomBack.X;
			}
		}

		public double Bottom
		{
			get
			{
				return LeftBottomBack.Y;
			}
		}

		public double Back
		{
			get
			{
				return LeftBottomBack.Z;
			}
		}

		public double Right
		{
			get
			{
				return RightTopFront.X;
			}
		}

		public double Top
		{
			get
			{
				return RightTopFront.Y;
			}
		}

		public double Front
		{
			get
			{
				return RightTopFront.Z;
			}
		}
	}
}
