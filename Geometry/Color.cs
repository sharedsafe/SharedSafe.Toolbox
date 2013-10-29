/**
	A color.
**/


namespace Toolbox.Geometry
{
	using Drawing = System.Drawing;

	public struct Color
	{
		public Color(double r, double g, double b)
			: this(r, g, b, 1.0)
		{
		}

		public Color(double r, double g, double b, double a)
		{
			Red = r;
			Green = g;
			Blue = b;
			Alpha = a;
		}

		public readonly double Red;
		public readonly double Green;
		public readonly double Blue;
		public readonly double Alpha;

		// shouldn't this be extension methods?

		public float RedF { get { return (float)Red; } }
		public float GreenF { get { return (float)Green; } }
		public float BlueF { get { return (float)Blue; } }
		public float AlphaF { get { return (float)Alpha; } }
	
		public static Color operator +(Color a, Color b)
		{
			return new Color(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue, a.Alpha + b.Alpha);
		}

		public static Color operator /(Color c, double f)
		{
			return c * (1.0 / f);
		}

		public static Color operator *(Color c, double f)
		{
			return new Color(c.Red * f, c.Green * f, c.Blue * f, c.Alpha * f);
		}

		public static Color mix(Color a, Color b)
		{
			return (a + b) / 2.0;
		}

		/**
			Support implicit conversion from a System.Drawing.Color to us!
		**/

		public static implicit operator Color(Drawing.Color c)
		{
			return new Color(c.R, c.G, c.B, c.A);
		}
	}

	
}
