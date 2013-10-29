using System;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ToolTipExtensions
	{
		/**
			Show a temporary hint below a control.
		**/

		public static void showHint(this ToolTip tt, Control control, string hint)
		{
			control.Focus();
			if (!control.Focused)
				return;

			string prevHint = tt.GetToolTip(control);

			EventHandler lostFocus = null;
			lostFocus = (sender, args) =>
				{
					control.LostFocus -= lostFocus;
					tt.Hide(control);
					tt.SetToolTip(control, prevHint);
				};

			control.LostFocus += lostFocus;

			tt.Show(hint, control, 0, control.Height, tt.AutoPopDelay);
		}

		public static void enableFor(this ToolTip tt, params Label[] labels)
		{
			foreach (var label in labels)
				tt.enableForLabel(label);
		}

		static void enableForLabel(this ToolTip tt, Label label)
		{
			Action refresh = () => refreshLabelTooltip(tt, label);

			label.TextChanged += delegate { refresh(); };
			label.SizeChanged += delegate { refresh(); };
		}

		static void refreshLabelTooltip(ToolTip tt, Label label)
		{
			tt.SetToolTip(label, label.PreferredWidth > label.Width ? label.Text : null);
		}
	}
}
