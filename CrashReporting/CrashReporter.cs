using System;
using System.Windows.Forms;

namespace Toolbox.CrashReporting
{
	public static class CrashReporter
	{
		public static void use(string reportURL, Action runApplication)
		{

			for (; ; )
			{
				try
				{
					runApplication();
					return;
				}
				catch (Exception e)
				{
					switch(report(e))
					{
						case PostReportingOptions.Exit:
							return;
						case PostReportingOptions.Restart:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		enum PostReportingOptions
		{
			Exit,
			Restart
		}

		static PostReportingOptions report(Exception e)
		{
			Application.EnableVisualStyles();
			var dialog = new CrashReporterForm(e);

			return dialog.ShowDialog() == DialogResult.Retry ? PostReportingOptions.Restart : PostReportingOptions.Exit;
		}
	}
}
