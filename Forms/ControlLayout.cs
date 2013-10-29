using System;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ControlLayout
	{
		/**
			Returns the container or one if its child if it has the focus.
		**/


		public static IDisposable suspendLayout(this Control control)
		{
			control.SuspendLayout();
			return new DisposeAction(control.ResumeLayout);
		}
	}
}
