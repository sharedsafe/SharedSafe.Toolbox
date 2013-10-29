using System.Collections.Generic;

namespace Toolbox.Sync
{
	public interface IDirtyPath
	{
		string Name { get; }

		/// Note: in case of leaf nodes, value must be null!
		Dictionary<string, IDirtyPath> Nested { get; }
	}

	static class DirtyExtensions
	{
		/// Simply sets the name dirty.

		public static void dirty(this IDirtyPath container, string name)
		{
			if (container.Nested.ContainsKey(name))
				return;

			container.Nested[name] = null;
		}

		/// Sets the name dirty and returns a dirty handle to it.

		public static IDirtyPath getDirty(this IDirtyPath container, string name)
		{
			IDirtyPath r;
			if (container.Nested.TryGetValue(name, out r) && r != null)
				return r;

			r = new Detail.DirtyPath(name);
			container.Nested[name] = r;
			return r;
		}

	}
}
