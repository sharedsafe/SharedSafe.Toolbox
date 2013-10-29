#if WPF
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Cursor=System.Windows.Forms.Cursor;
using System.Drawing;

namespace Toolbox.WPF
{
	static class NativeMouse
	{
		const int Left = 0x2;
		const int Right = 0x8;
		const int Middle = 0x20;

		#region Properties

		/// <summary>
		/// Gets or sets a structure that represents both X and Y mouse coordinates
		/// </summary>
		public static Point Position
		{
			get
			{
				var pos = Cursor.Position;
				return new Point(pos.X, pos.Y);
			}
			set
			{
				Cursor.Position = value;
			}
		}

		#endregion

		#region Methods

		static void down(int button)
		{
			mouse_event(button, 0, 0, 0, 0);
		}

		public static void down(MouseButton button)
		{
			down(nativate(button));
		}

		static void up(int button)
		{
			mouse_event(button << 1, 0, 0, 0, 0);
		}

		public static void up(MouseButton button)
		{
			click(nativate(button));
		}

		static void click(int button)
		{
			down(button);
			up(button);
		}

		public static void click(MouseButton button)
		{
			click(nativate(button));
		}

		static void doubleClick(int button)
		{
			click(button);
			click(button);
		}

		public static void doubleClick(MouseButton button)
		{
			doubleClick(nativate(button));
		}

		static int nativate(MouseButton button)
		{
			switch (button)
			{
				case MouseButton.Left:
					return Left;
				case MouseButton.Middle:
					return Middle;
				case MouseButton.Right:
					return Right;
				default:
					throw new NotImplementedException(button.ToString());
			}
		}

		public static void rollWheel(int steps)
		{
			mouse_event(MOUSEEVENTF_WHEEL, 0, 0, steps*120, 0);
		}
		
		public static void Show()
		{
			ShowCursor(true);
		}

		public static void Hide()
		{
			ShowCursor(false);
		}

		#endregion

		#region Windows API

		[DllImport("user32.dll")]
		static extern int ShowCursor(bool show);

		[DllImport("user32.dll")]
		static extern void mouse_event(int flags, int dX, int dY, int buttons, int extraInfo);

		const int MOUSEEVENTF_MOVE = 0x1;
		const int MOUSEEVENTF_WHEEL = 0x800;
		const int MOUSEEVENTF_ABSOLUTE = 0x8000;

		#endregion

	}
}
#endif
