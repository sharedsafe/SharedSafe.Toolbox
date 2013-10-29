using System;
using System.Windows.Forms;

namespace Toolbox.CrashReporting
{
	public partial class CrashReporterForm : Form
	{
		readonly Exception _exception;

		public CrashReporterForm(Exception e)
		{
			_exception = e;

			InitializeComponent();

			Text = "{0} Crash Reporter".format(Application.ProductName);

			Sorry.Text =
				"Sorry, {0} crashed and can not be continued.\nPlease report this problem to help us improving {0}.".format(
					Application.ProductName);
		}

		private void Details_Click(object sender, EventArgs e)
		{
			var details = new CrashReporterDetails(_exception);
			details.ShowDialog();
		}
	}
}
