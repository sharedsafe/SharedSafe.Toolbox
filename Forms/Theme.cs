using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace Toolbox.Forms
{
	public static class Theme
	{
		public static readonly Color GroupBoxTextColor = makeGroupBoxTextColor();

		static Color makeGroupBoxTextColor()
		{
			var vse = VisualStyleElement.Button.GroupBox.Normal;
			var renderer = new VisualStyleRenderer(vse);
			return renderer.GetColor(ColorProperty.TextColor);
		}
	}
}
