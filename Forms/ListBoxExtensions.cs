using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	public static class ListBoxExtensions
	{
		/// Calls the provided action with the instance of the selected item (if it fits)

		public static bool with<ItemT>(this ListBox lb, Action<ItemT> action)
		{
			var index = lb.SelectedIndex;
			if (index == -1)
				return false;

			action((ItemT)lb.Items[index]);
			return true;
		}

		public static void updateItemSorted(this ListBox lb, object item)
		{
			using (lb.update())
			{
				lb.updateItem(item);
				lb.sort();
			}
		}

		public static void updateItem(this ListBox lb, object item)
		{
			var index = lb.Items.IndexOf(item);
			Debug.Assert(index != -1);

			using (lb.updatingContent())
				lb.Items[index] = item;
		}


		public static void sort(this ListBox lb)
		{
			// note: sorting may also call back SelectedIndexChanged multiple times.
			using (lb.updatingContent())
			{
				// this ensures calling the protected virtual function sort()
				lb.Sorted = false;
				lb.Sorted = true;
			}
		}

		public static void deleteItem(this ListBox lb, object item)
		{
			int i = lb.Items.IndexOf(item);
			Debug.Assert(i != -1);

			using (lb.update())
			{
				bool reselect = false;

				if (lb.SelectedIndex == i)
				{
					lb.ClearSelected();
					reselect = true;
				}

				lb.Items.RemoveAt(i);

				if (!reselect)
					return;

				if (i < lb.Items.Count)
					lb.SelectedIndex = i;
				else if (lb.Items.Count != 0)
					lb.SelectedIndex = lb.Items.Count - 1;
			}
		}
		static readonly Dictionary<ListBox, uint> UpdatingContent = new Dictionary<ListBox, uint>();

		public static IDisposable updatingContent(this ListBox lb)
		{
			if (!UpdatingContent.ContainsKey(lb))
				UpdatingContent.Add(lb, 1);
			else
				++UpdatingContent[lb];

			return new DisposeAction(() =>
			{
				if (--UpdatingContent[lb] == 0)
					UpdatingContent.Remove(lb);
			});
		}


		public static void setIndexChangedHandler(this ListBox lb, Action changeHandler)
		{
			lb.SelectedIndexChanged += (sender, args) =>
			{
				if (UpdatingContent.ContainsKey(lb))
					return;

				changeHandler();
			};
		}

		public static IDisposable update(this ListBox lb)
		{
			lb.BeginUpdate();

			return new DisposeAction(() => lb.EndUpdate());
		}
	}
}
