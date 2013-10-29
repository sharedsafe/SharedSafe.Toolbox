namespace Toolbox.Geometry
{
	static class Vectors
	{

		public static Vector[] translate(Vector[] vectors, Vector translate)
		{
			Vector[] r = new Vector[vectors.Length];

			for (int i = 0; i!= vectors.Length; ++i)
				r[i] = vectors[i] + translate;

			return r;
		}
	}
}
