using System;
using System.Windows.Forms;
using Toolbox.Timing;

namespace Toolbox.Forms
{
	public static class ApplicationExtensions
	{
		public static IDisposable onIdle(Action a)
		{
			EventHandler runOnIdle = (sender, args) => a();
			Application.Idle += runOnIdle;
			return new DisposeAction(() => Application.Idle -= runOnIdle);
		}
	}
}
