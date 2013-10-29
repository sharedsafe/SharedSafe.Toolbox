using System;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class FormExtensions
	{
		/**
			Temporarily shows a modal dialog topmost (there is some problem, if we don't set a parent, that
			the dialog comes up below other windows when it is started from tray or other processes.
		**/

		public static DialogResult showDialogTopmost(this Form form)
		{
			// hack when running the wizard from the tray icon we
			// do have a problem moving the dialog to the front (don't know why, bringtoFront does not work
			// it is somehow placed behind all other windows)
			form.TopMost = true;

			form.Shown += clearTopMost;
			var res = form.ShowDialog();
			form.Shown -= clearTopMost;
			return res;
		}

		static void clearTopMost(object sender, EventArgs args)
		{
			((Form)sender).TopMost = false;
		}
	}
}
