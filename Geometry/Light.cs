namespace Toolbox.Geometry
{
	public struct Light
	{
		public Light(Vector position, Color ambient, Color diffuse, Color specular)
		{
			Position = position;
			Ambient = ambient;
			Diffuse = diffuse;
			Specular = specular;
		}

		public readonly Vector Position;
		public readonly Color Ambient;
		public readonly Color Diffuse;
		public readonly Color Specular;
	}
}
