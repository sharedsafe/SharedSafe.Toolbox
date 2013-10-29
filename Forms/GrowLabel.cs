/**
	http://social.msdn.microsoft.com/forums/en-US/winforms/thread/97c18a1d-729e-4a68-8223-0fcc9ab9012b/
**/

using System;
using System.Windows.Forms;
using System.Drawing;

namespace Toolbox.Forms
{
	public sealed class GrowLabel : Label
	{
		private bool mGrowing;
		public GrowLabel()
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
