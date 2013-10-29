using System;
using System.Drawing;

namespace Toolbox.Drawing
{
	public static class ColorExtensions
	{

		// I think this makes no sense, is not used and additional, is counter-intuitive!
#if false
		public static Color mix(this Color c, double f)
		{
			return c.mix(new Color(), f);
		}
#endif
		public static Color mix(this Color c, Color d, double f)
		{
			return Color.FromArgb(
				interpolate(c.A, d.A, f),
				interpolate(c.R, d.R, f),
				interpolate(c.G, d.G, f),
				interpolate(c.B, d.B, f));
		}

		static byte interpolate(byte a, byte b, double f)
		{
			return clamp((byte)Math.Round(a * (1.0 - f) + b * f), 0, 255);
		}

		static byte clamp(byte b, byte low, byte high)
		{
			if (b < low)
				return low;

			if (b > high)
				return high;

			return b;
		}

		public static Color darken(this Color c, double f)
		{
			var other = Color.FromArgb(c.A, 0, 0, 0);
			return mix(c, other, f);
		}

	}
}
