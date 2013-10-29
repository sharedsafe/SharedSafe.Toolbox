using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainModeling.Tools
{
	public sealed class ModificationTracker<TypeT> : IDisposable
	{
		readonly Action<TypeT[]> _callback;
		readonly HashSet<TypeT> _modified = new HashSet<TypeT>();

		public ModificationTracker(Action<TypeT[]> callback)
		{
			_callback = callback;
		}

		public void modified(TypeT instance)
		{
			_modified.Add(instance);
		}

		public void Dispose()
		{
			if (_modified.Count != 0)
				_callback(_modified.ToArray());
		}
	}
}
