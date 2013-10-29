using System;
using System.Windows.Forms;

namespace Toolbox.CrashReporting
{
	public partial class CrashReporterDetails : Form
	{
		public CrashReporterDetails(Exception e)
		{
			InitializeComponent();

			Description.Text = e.Message;
			StackTrace.Text = e.StackTrace;
		}
	}
}
