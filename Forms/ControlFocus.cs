using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ControlFocus
	{
		public static bool tryFocusFirstTextBox(this Control container)
		{
			return tryFocusFirstControl(container, c => c is TextBox && !((TextBox)c).ReadOnly);
		}

		public static bool tryFocusFirstControl(this Control container)
		{
			return tryFocusFirstControl(container, c => true);
		}

		public static bool tryFocusFirstControl(this Control container, Func<Control, bool> predicate)
		{
			var controls = from c in container.focusableChildren() where predicate(c) select c;

			foreach (var c in controls)
			{
				if (c.Focus())
					return true;
			}

			return false;
		}

		public static bool tryFocusNextControl(this Control container)
		{
			bool found = false;

			foreach (var control in container.focusableChildren())
			{
				if (!found)
				{
					if (control.Focused)
						found = true;
				}
				else
				{
					if (control.Focus())
						return true;
				}
			}

			return false;
		}

		public static Control tryFindFocusedControl(this Control container)
		{
			return (isFocusableControlContainer(container) && container.Focused) ? container : tryFindFocusedChild(container);
		}

		public static Control tryFindFocusedChild(this Control container)
		{
			return container.focusableChildren().FirstOrDefault(c => c.Focused);
		}

		static IEnumerable<Control> focusableChildren(this Control container)
		{
			if (!isFocusableControlContainer(container))
				yield break;

			var nestedControls = from c in container.Controls.enumerate() where c.Visible && c.Enabled orderby c.TabIndex select c;

			foreach (var nested in nestedControls)
			{
				if (nested.Controls.Count != 0)
				{
					foreach (var n in nested.focusableChildren())
						yield return n;
				}
				else
					if (nested.TabStop)
						yield return nested;
			}
		}

		static bool isFocusableControlContainer(this Control control)
		{
			return control.Visible && control.Enabled;
		}
	}
}
