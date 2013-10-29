#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NUnit.Framework;

namespace Toolbox.WPF
{
#if DEBUG

	[TestFixture]
	public class Tests
	{
		readonly Application app = new Application();
		readonly Window _win;

		public Tests()
		{
			_win = new Window();
			_win.Left = 0;
			_win.Top = 0;
			_win.Width = 100;
			_win.Height = 100;
		}

		[Test]
		public void sendMouseDownAndUp()
		{
			_win.Hide();

			var canvas = new Canvas();
			canvas.Background = new SolidColorBrush(Colors.Black);
			_win.Content = canvas;
			_win.Show();

			bool mouseDown = false;
			Point atPos = new Point(0,0);

			canvas.MouseDown += (s, args) =>
				{
					mouseDown = true;
					atPos = args.GetPosition(canvas);
				};

			bool mouseUp = false;

			canvas.MouseUp += (s, args) =>
				{
					mouseUp = true;
					atPos = args.GetPosition(canvas);
				};

			var p = new Point(10, 10);

			canvas.simulateMouseDown(MouseButton.Left, p);

			Assert.True(mouseDown);
			Assert.AreEqual(p, atPos);

			canvas.simulateMouseUp(MouseButton.Left, p);

			Assert.True(mouseUp);
			Assert.AreEqual(p, atPos);
		}

		[Test]
		public void testCentering()
		{
			_win.Hide();

			var canvas = new Canvas();
			canvas.Background = new SolidColorBrush(Colors.Black);
			_win.Content = canvas;

			_win.Show();

			Point atPos = new Point(0,0);

			bool mouseDown = false;

			canvas.MouseDown += (s, args) =>
			{
				mouseDown = true;
				atPos = args.GetPosition(canvas);
			};

			canvas.simulateMouseDown(MouseButton.Left);

			Assert.True(mouseDown);
			Assert.AreEqual(canvas.ActualWidth / 2.0, atPos.X);
			Assert.AreEqual(canvas.ActualHeight / 2.0, atPos.Y);
		}
	}




#endif
}
#endif
