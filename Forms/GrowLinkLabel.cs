using System;
using System.Drawing;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public sealed class GrowLinkLabel : LinkLabel
	{
		private bool mGrowing;
		public GrowLinkLabel()
		{
			AutoSize = false;
		}

		private void resizeLabel()
		{
			if (mGrowing) return;
			try
			{
				mGrowing = true;
				var sz = new Size(ClientSize.Width - Padding.Horizontal, Int32.MaxValue);
				sz = TextRenderer.MeasureText(Text, Font, sz,
					TextFormatFlags.TextBoxControl |
						TextFormatFlags.WordBreak);

				// respect minimum height!
				// sz.Height = Math.Min(MinimumSize.Height, sz.Height);

				// note: this must reduce height also, otherwise resizing the space to fit 
				// more text would not decrease height!

				ClientSize = new Size(ClientSize.Width, sz.Height + Padding.Vertical);
			}
			finally
			{
				mGrowing = false;
			}
		}
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			resizeLabel();
		}
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			resizeLabel();
		}
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			resizeLabel();
		}
		protected override void OnPaddingChanged(EventArgs e)
		{
			base.OnPaddingChanged(e);
			resizeLabel();
		}
	}
}