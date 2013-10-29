using System.Drawing;

namespace Toolbox
{
	public struct Percentage
	{
		readonly double Factor;

		internal Percentage(double factor)
		{
			Factor = factor;
		}

		// Must provide int here, otherwise overload resolution would choose double for uints

		internal Percentage(int percent)
			: this(percent / 100d)
		{
		}

		internal Percentage(uint percent)
			: this((int)percent)
		{
		}

		public static Percentage operator +(Percentage l, Percentage r)
		{
			return new Percentage(l.Factor + r.Factor);
		}

		public static Percentage operator -(Percentage l, Percentage r)
		{
			return l + -r;
		}

		public static Percentage operator -(Percentage l)
		{
			return new Percentage(-l.Factor);
		}

		public static Percentage operator *(Percentage l, Percentage r)
		{
			return new Percentage(l.Factor * r.Factor);
		}

		public static Percentage operator %(Percentage l, Percentage r)
		{
			return new Percentage(l.Factor % r.Factor);
		}

		public static Percentage operator /(Percentage l, Percentage r)
		{
			return new Percentage(l.Factor / r.Factor);
		}

		#region Interopability with other types

#if !TOOLBOX_ESSENTIALS

		public static Size operator *(Size size, Percentage p)
		{
			return new Size(size.Width * p, size.Height * p);
		}
#endif

		public static double operator *(double d, Percentage p)
		{
			return d * p.Factor;
		}

		public static uint operator *(uint d, Percentage p)
		{
			return (uint)(d * p.Factor);
		}

		public static int operator *(int d, Percentage p)
		{
			return (int)(d * p.Factor);
		}

		#endregion
	};

	public static class PercentageExtensions
	{
		public static Percentage percent(this uint value)
		{
			return new Percentage(value);
		}

		public static Percentage percent(this int value)
		{
			return new Percentage(value);
		}
	}
}
