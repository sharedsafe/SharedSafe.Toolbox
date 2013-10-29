/**
	Instantiate FormManager in your form to support common Form interactions.
**/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public sealed class FormManager
	{
		readonly Dictionary<Control, Label> _labels = new Dictionary<Control, Label>();

		public void label(TextBox tb, Label lb, string pattern)
		{
			var re = new Regex(
				pattern, 
				RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

			label(tb, lb, str => re.Match(str).Success);
		}

		public void label(TextBox tb, Label lb, Func<string, bool> verify)
		{
			var defaultLabelForeColor = lb.ForeColor;

			Action a = () =>
				{
					bool ok = verify(tb.Text);
					lb.ForeColor = ok ? defaultLabelForeColor : Color.Red;
				};

			tb.TextChanged += (s, args) => a();

			label(tb, lb);

			a();
		}

		public void label(Control control, Label label)
		{
			_labels.Add(control, label);
		}
	}
}
