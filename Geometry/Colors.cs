namespace Toolbox.Geometry
{
	public static class Colors
	{
		public static readonly Color Black = c(0.0, 0.0, 0.0);
		public static readonly Color White = c(1.0, 1.0, 1.0);
		public static readonly Color Red = c(1.0, 0.0, 0.0);
		public static readonly Color Green = c(0.0, 1.0, 0.0);
		public static readonly Color Blue = c(0.0, 0.0, 1.0);
		public static readonly Color Transparent = new Color();

		static Color c(double r, double g, double b)
		{
			return new Color(r, g, b);
		}
	}
}
