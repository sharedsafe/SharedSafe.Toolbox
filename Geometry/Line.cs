/**
	A line, described by a begin and a end position.
**/

namespace Toolbox.Geometry
{
	public struct Line
	{
		public readonly Vector Begin;
		public readonly Vector End;

		public Line(Vector begin, Vector end)
		{
			Begin = begin;
			End = end;
		}

		public bool IsPoint
		{
			get
			{
				return Begin == End;
			}
		}

		public double Length
		{
			get
			{
				return Orientation.Length;
			}
		}

		public Vector Orientation
		{
			get
			{
				return End - Begin;
			}
		}


		/// Multiply line with vector.
		
		public static Line operator *(Line ray, Matrix m)
		{
			return new Line(ray.Begin * m, ray.End * m);
		}

	}
}
