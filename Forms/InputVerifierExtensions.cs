using System;
using System.Drawing;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class InputVerifierExtensions
	{
		public static bool verifyAndHint(this IInputVerifier verifier)
		{
			if (verifier.verify())
				return true;

			verifier.highlightAndFocusFirstUnverified();
			return false;
		}

		public static void ensure(this IInputVerifier verifier, TextBox tb, Func<string, bool> verify)
		{
			tb.ensure(verify);

			var back = tb.BackColor;
			verifier.register(() => verify(tb.Text), () =>
			{
				tb.BackColor = SystemColors.Highlight;
				tb.animateColorProperty(textBox => textBox.BackColor, back, 200);
			}, () => tb.Focus());
		}
	}
}
