using System.Diagnostics;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public sealed class LiveSplitter : Splitter
	{
		int _startX;

		protected override void OnSplitterMoving(SplitterEventArgs sevent)
		{
			SplitPosition = sevent.X - _startX;
		}

		protected override void OnSplitterMoved(SplitterEventArgs sevent)
		{
			// override moved to do nothing, otherwise control would be confused a bit.
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			// this _will_ start the drag operation, so we save the start position.
			if (e.Button == MouseButtons.Left && e.Clicks == 1)
				_startX = e.X + Margin.Left;

			base.OnMouseDown(e);
		}
	}
}
