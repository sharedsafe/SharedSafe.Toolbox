#if WPF
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using D = System.Drawing;
using F = System.Windows.Forms;

namespace Toolbox.WPF
{
	public static class SimulationExtensions
	{
		public static void simulateMouseUp(this FrameworkElement target, MouseButton button)
		{
			target.simulateMouseUp(button, center(target));
		}

		public static void simulateMouseDown(this FrameworkElement target, MouseButton button)
		{
			target.simulateMouseDown(button, center(target));
		}

		static Point center(FrameworkElement element)
		{
			dispatchPendingEvents();
			return new Point(element.ActualWidth / 2.0, element.ActualHeight / 2.0);
		}

		public static void simulateMouseUp(this UIElement target, MouseButton button, Point where)
		{
			Action a = () => NativeMouse.up(button);
			simulateMouseEvent(target, where, a);
		}

		public static void simulateMouseDown(this UIElement target, MouseButton button, Point where)
		{
			Action a = () => NativeMouse.down(button);
			simulateMouseEvent(target, where, a);
		}

		public static void simulateMouseEvent(this UIElement target, Point where, Action action)
		{
			dispatchPendingEvents();

			var screenPos = target.PointToScreen(where);
			F.Cursor.Position = new D.Point(screenPos.X.rounded(), screenPos.Y.rounded());
			action();
	
			dispatchPendingEvents();
		}

		static void dispatchPendingEvents()
		{
			var disp = Dispatcher.CurrentDispatcher;
			disp.BeginInvoke(DispatcherPriority.Background, (Action)Dispatcher.ExitAllFrames);
			Dispatcher.Run();
		}
	}
}
#endif