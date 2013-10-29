#if WPF

using System.Windows.Controls;

namespace Toolbox.WPF
{
	public static class TextBoxExtensions
	{
		public static void insert(this TextBox tb, string text)
		{
			using (tb.DeclareChangeBlock())
			{
				tb.deleteSelection();
				var caretIndex = tb.CaretIndex;
				tb.Text = tb.Text.Insert(caretIndex, text);
				tb.CaretIndex = caretIndex + text.Length;
			}
		}

		public static void deleteSelection(this TextBox tb)
		{
			if (tb.SelectionLength == 0)
				return;

			var startOfSelection = tb.SelectionStart;

			tb.Text = tb.Text.Remove(startOfSelection, tb.SelectionLength);
			tb.CaretIndex = startOfSelection;
		}
	}
}

#endif
