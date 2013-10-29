using System.Collections.Generic;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ControlCollectionExtensions
	{
		public static IEnumerable<Control> enumerate(this Control.ControlCollection collection)
		{
			foreach (var c in collection)
				yield return (Control) c;
		}
	}
}
