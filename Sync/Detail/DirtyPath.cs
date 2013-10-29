using System.Collections.Generic;

namespace Toolbox.Sync.Detail
{
	sealed class DirtyPath : IDirtyPath
	{
		Dictionary<string, IDirtyPath> _nested;

		public DirtyPath(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public Dictionary<string, IDirtyPath> Nested
		{
			get { return _nested ?? (_nested = new Dictionary<string, IDirtyPath>()); }
		}

		public override string ToString()
		{
			return _nested != null && _nested.Count != 0 ? Name + " (" + _nested.Count + ")" : Name;
		}
	}
}
