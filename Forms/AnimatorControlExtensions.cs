using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Windows.Forms;
using Toolbox.Meta;

namespace Toolbox.Forms
{
	public static class AnimatorControlExtensions
	{
		public static void animateColorProperty<ControlT>(this ControlT control, Expression<Func<ControlT, Color>> target, Color final, uint milliseconds)
			where ControlT : Control
		{
			MemberAccessor<Color> memberAccessor = target.toMemberAccessor().resolve(() => control);
			var animation = new Animation<ControlT, Color>(control, memberAccessor, final, milliseconds, exp<Color>(interpolate));
			Animator.ThreadLocalInstance.add(animation);
		}


		static double exp(double f)
		{
			var f1 = 1.0 - f;
			return 1.0 - (f1 * f1 * f1);
		}

		static Func<ValueT, ValueT, double, ValueT> exp<ValueT>(Func<ValueT, ValueT, double, ValueT> interpolator)
		{
			return (a, b, f) => interpolator(a, b, exp(f));
		}


		public static Color interpolate(Color l, Color r, double f)
		{
			var c = Color.FromArgb(
				mixB(l.A, r.A, f),
				mixB(l.R, r.R, f),
				mixB(l.G, r.G, f),
				mixB(l.B, r.B, f));

			return c;
		}

		public static byte mixB(byte l, byte r, double f)
		{
			var value = (byte)Math.Round(l * (1.0 - f) + r * f);
			if (value < 0)
				value = 0;
			if (value > 255)
				return 255;

			return value;
		}
	}
}