/**
	A shape.

	A shape is an geometrical figure described by a number of vertices and polygons.

**/

namespace Toolbox.Geometry
{
	public sealed class Shape
	{
		public readonly uint[][] Faces;
		public readonly Vector[] Vertices;

		/**
			Full constructor.
		**/

		public Shape(
			uint[][] faces,
			Vector[] vertices)
		{
			Faces = faces;
			Vertices = vertices;
		}
	}
}

