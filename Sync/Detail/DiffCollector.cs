using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	using DiffRecord = Pair<string, DiffKind>;

	/**
		Collects dirty hierarchy points.
	**/

	sealed class DiffCollector : IDiffCollector, IKnowledgeDiff
	{
		readonly List<Pair<IScope, DiffRecord>> _diffs = new List<Pair<IScope, DiffRecord>>();

		public SyncOptions Options { get; private set; }
		public DiffCollector(SyncOptions options)
		{
			Options = options;
		}

		public void compare(IScope scope, IItem source, IItem target)
		{
			Debug.Assert(source.Type == target.Type);
			Options.Types[source.Type.index()].diff(scope, source, target, this);
		}

		/**
			Note: It is expected here that 
		
			a) scopes representing the same path are also the same instance.
			b) root scopes all in the same IScope.
		**/

		public void collect(IScope scope, string name, DiffKind kind)
		{
#if DEBUG
			// assert b)

			if (_diffs.Count != 0)
				Debug.Assert(_diffs[0].First.getRoot() == scope.getRoot());
#endif

			_diffs.Add(Pair.make(scope, Pair.make(name, kind)));
		}

		#region Queries

		public bool AnyDiffs
		{ get { return _diffs.Count != 0; } }

		/**
			Create a dirty path representation. 
		
			Always returns a dirty root with name == string.Empty;
			If no paths are dirty, returns a dirty root with no children.
		**/

		public IDirtyPath createDirtyPath()
		{
			/// All recorded scopes actually represent an invalid directory.
			
			var map = new Dictionary<IScope, IDirtyPath>();

			foreach (var dirty in _diffs)
			{
				var path = resolveToMap(map, dirty.First);
				addToPath(path, dirty.Second.First);
			}

			return map.Count == 0 ? createDirtyRoot() : map[map.Keys.First().getRoot()];
		}

		/**
			Dump all out all recorded diffs.
		**/

		public IEnumerable<string> dump()
		{
			foreach (var diff in _diffs)
			{
				// lol, pair stuff:
				yield return diff.Second.Second + ": " + diff.First.makePath(diff.Second.First);
			}
		}

		#endregion

		#region Helpers

		/**
			Lookup and/or create all required (up-closed) dirty entries.
		**/

		static IDirtyPath resolveToMap(IDictionary<IScope, IDirtyPath> map, IScope it)
		{
			IDirtyPath path;
			if (map.TryGetValue(it, out path))
				return path;

			if (it.Parent_ == null)
			{
				// need root :)
				var root = createDirtyRoot();
				map[it] = root;
				return root;
			}

			var name = it.Name;

			var parent = resolveToMap(map, it.Parent_);
			var newPath = new DirtyPath(name);

			// there might be one existing, but it shall be null then, we leave that to the test cases to track
			// this situation down.
			// todo: please answer why null is happening (this happens since we track deep news in dirty paths)
			Debug.Assert(!parent.Nested.ContainsKey(name) || parent.Nested[name] == null);
	
			parent.Nested[name] = newPath;
			map[it] = newPath;

			return newPath;
		}

		static void addToPath(IDirtyPath path, string name)
		{
			// already dirty?
			if (path.Nested.ContainsKey(name))
				return;

			path.Nested[name] = null;
		}

		static IDirtyPath createDirtyRoot()
		{
			return new DirtyPath(string.Empty);
		}

		#endregion
	}
}
