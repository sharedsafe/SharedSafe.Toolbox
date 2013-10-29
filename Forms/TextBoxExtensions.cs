using System;
using System.Drawing;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class TextBoxExtensions
	{
		public static void ensure(this TextBox tb, Func<string, bool> verify)
		{
			var c = tb.ForeColor;
			EventHandler f = (s, args) => ensureTB(tb, c, verify);

			tb.TextChanged += f;
		}

		public static void ensureTB(TextBox tb, Color foreColor, Func<string, bool> verify)
		{
			var tx = tb.Text;
			bool r = false;
			try
			{
				r = verify(tx);
			}
			catch (Exception e)
			{
				Log.E(e.Message);
			}

			tb.ForeColor = r ? foreColor : Color.Red;
		}

	}
}
