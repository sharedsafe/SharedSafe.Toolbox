using System;
using System.Drawing;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	/// Adds link behavior to a label.

	public sealed class LinkBehavior : IDisposable
	{
		readonly Label _label;
		Font _regularFont;
		Font _underlinedFont;
		bool _entered;
		bool _over;

		LinkBehavior(Label label)
		{
			_label = label;

			_label.ForeColor = Color.FromArgb(255, 0, 0, 255);

			_label.MouseEnter += mouseEnter;
			_label.MouseMove += mouseMove;
			_label.MouseLeave += mouseLeave;

			_label.MouseDown += mouseDown;
			_label.MouseUp += mouseUp;
		}

		public void Dispose()
		{
			_label.MouseUp -= mouseUp;
			_label.MouseDown -= mouseDown;

			_label.MouseLeave -= mouseLeave;
			_label.MouseMove -= mouseMove;
			_label.MouseEnter -= mouseEnter;

			if (_underlinedFont != null)
				_underlinedFont.Dispose();
		}

		void mouseEnter(object s, EventArgs a)
		{
			if (_regularFont == null)
			{
				_regularFont = _label.Font;
				_underlinedFont = new Font(_regularFont, FontStyle.Underline);
			}

			_label.Font = _underlinedFont;
			_entered = true;
			_over = true;
			_label.Cursor = Cursors.Hand;
		}


		void mouseMove(object s, MouseEventArgs a)
		{
			if (!_entered || _regularFont == null)
				return;


			bool over = a.Location.X >= 0 && a.Location.Y >= 0 && a.Location.X < _label.Size.Width && a.Location.Y < _label.Size.Height;

			if (over == _over)
				return;

			_label.Font = over ? _underlinedFont : _regularFont;
			_label.Cursor = over ? Cursors.Hand : Cursors.Default;
			_over = over;
		}

		void mouseLeave(object s, EventArgs a)
		{
			_entered = false;
			if (_regularFont != null)
				_label.Font = _regularFont;

			_label.ForeColor = Color.FromArgb(255, 0, 0, 255);
			_label.Cursor = Cursors.Default;
		}

		void mouseDown(object s, MouseEventArgs a)
		{
			_label.ForeColor = Color.Red;
		}

		void mouseUp(object s, MouseEventArgs a)
		{
			_label.ForeColor = Color.FromArgb(255, 0, 0, 255);
		}

		public static IDisposable bind(Label label)
		{
			return new LinkBehavior(label);
		}
	}
}
