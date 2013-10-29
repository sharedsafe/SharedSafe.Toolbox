using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Toolbox.Forms
{
	/**
		Note: this is essentially a copy of ListBoxExtensions, but we don't know how to reuse the code.
	**/

	public static class ComboBoxExtensions
	{
		/// Calls the provided action with the instance of the selected item (if it fits)

		public static bool with<ItemT>(this ComboBox cb, Action<ItemT> action)
		{
			var index = cb.SelectedIndex;
			if (index == -1)
				return false;

			action((ItemT)cb.Items[index]);
			return true;
		}

		public static void updateItemSorted(this ComboBox cb, object item)
		{
			using (cb.update())
			{
				cb.updateItem(item);
				cb.sort();
			}
		}

		public static void updateItem(this ComboBox cb, object item)
		{
			var index = cb.Items.IndexOf(item);
			Debug.Assert(index != -1);

			using (cb.updatingContent())
			{
				Log.D("CB: updating item at index {0}".format(index));
				cb.Items[index] = item;
				Log.D("CB: updating item end");
			}
		}


		public static void sort(this ComboBox cb)
		{
			using (cb.updatingContent())
			{
				Log.D("CB: sorting");

				// note: combobox loses SelectedItem on resorting, so we must care!!!!!
				var item = cb.SelectedItem;

				try
				{
					// this ensures calling the protected virtual function sort()
					cb.Sorted = false;
					cb.Sorted = true;
				}
				finally
				{
					cb.SelectedItem = item;
					Log.D("CB: sorting end");
				}
			}
		}

		public static void deleteItem(this ComboBox cb, object item)
		{
			int i = cb.Items.IndexOf(item);
			Debug.Assert(i != -1);

			using (cb.update())
			{
				bool reselect = false;

				if (cb.SelectedIndex == i)
				{
					cb.SelectedIndex = -1;
					reselect = true;
				}

				cb.Items.RemoveAt(i);

				if (!reselect)
					return;

				if (i < cb.Items.Count)
					cb.SelectedIndex = i;
				else if (cb.Items.Count != 0)
					cb.SelectedIndex = cb.Items.Count - 1;
			}
		}

		static readonly Dictionary<ComboBox, uint> UpdatingContent = new Dictionary<ComboBox, uint>();

		public static IDisposable updatingContent(this ComboBox cb)
		{
			if (!UpdatingContent.ContainsKey(cb))
				UpdatingContent.Add(cb, 1);
			else
				++UpdatingContent[cb];

			return new DisposeAction(() =>
				{
					if (--UpdatingContent[cb] == 0)
						UpdatingContent.Remove(cb);
				});
		}

		public static void setIndexChangedHandler(this ComboBox cb, Action changeHandler)
		{
			cb.SelectedIndexChanged += (sender, args) =>
			{
				if (UpdatingContent.ContainsKey(cb))
				{
					Log.D("Suppressing selected index changed to {0} on ComboBox {1}".format(cb.SelectedIndex, cb));

					return;
				}

				changeHandler();
			};
		}


		public static IDisposable update(this ComboBox cb)
		{
			cb.BeginUpdate();

			return new DisposeAction(() => cb.EndUpdate());
		}
	}
}
