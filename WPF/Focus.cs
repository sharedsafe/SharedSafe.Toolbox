#if WPF

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Toolbox.WPF
{
	public static class Focus
	{
		/*
			In brainsharper we use the Loaded event to set the focus, 
			but this may not work if the control just was invisible before.
		*/

		[Obsolete("Use setFocusSafe")]
		public static void setFocusLate(this Control control)
		{
			DependencyPropertyChangedEventHandler handler = null;

			handler = delegate
				{
					control.Focus();
					control.IsVisibleChanged -= handler;
				};

			control.IsVisibleChanged += handler;
		}

		public static void setFocusSafe(this Control control)
		{
			Debug.Assert(control.Focusable);
			Action focus = () => Keyboard.Focus(control);

			if (!control.IsLoaded)
			{
				RoutedEventHandler action = null;

				action = (sender, ev) =>
					{
						focus();
						control.Loaded -= action;
					};

				control.Loaded += action;
			}
			else
				control.Dispatcher.BeginInvoke(focus, DispatcherPriority.Render);
		}
	}
}
#endif