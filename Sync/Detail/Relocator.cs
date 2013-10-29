using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	sealed class Relocator : IRelocator, IRelocationContext
	{
		readonly IItemType[] _types;
		public IReplica[] Replicas { get; private set; }
		public SyncOptions Options { get; private set;}
		IScope _currentScope_;

		public Relocator(params IReplica[] replicas)
		{
			Replicas = replicas;

			SyncOptions.requireAllSame(from r in replicas select r.Options);
			Options = replicas[0].Options;
			_types = Options.Types;
		}

		#region IRelocationContext Members

		// note: actual items may be null (for example in NativeFileSystem.CreateFile()
		// this has to be tested independently!
		// todo: may need a more sophisticated error handling here.

		public RelocationError recordError(string description, IEnumerable<Pair<IItem, string>> items, Exception e_)
		{
			this.I(description);
			foreach (var item in items)
			{
				var i_ = item.First;
				this.D((i_ == null ? "[no item]" : i_.ToString()) + " " + item.Second.quote());
			}
			if (e_ != null)
				this.I(e_);

			return new RelocationError(description, items.ToArray(), e_);
		}

		#endregion

		#region IRelocator Members

		public IDisposable push(IScope scope)
		{
#if DEBUG_SCOPE
			this.I("++ (scope) " + scope.ToString());
#endif

			Debug.Assert(scope != null && scope.Parent_ == _currentScope_);
			_currentScope_ = scope;

			var exit = enter(scope);

			return new DisposeAction(() =>
				{

					Debug.Assert(_currentScope_ == scope);

					exit.Dispose();
					_currentScope_ = scope.Parent_;

#if DEBUG_SCOPE
					this.I("-- (scope) " + scope);
#endif
				});
		}

		public bool tryReconcile(IItem[] items, uint i)
		{
			try
			{
				reconcile(items, i);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void reconcile(IItem[] items, uint i)
		{
			Debug.Assert(items.Length == 2);
			Debug.Assert(i < items.Length);

			if (items[i] == null)
				deleteAllExcept(items, i);
			else 
				copySafe(items, i);
		}

		void deleteAllExcept(IItem[] items, uint index)
		{
			foreach (var i in items.unsignedIndices())
			{
				if (i == index || items[i] == null)
					continue;

				var item = items[i];

				var ctx = new ItemContext(item.Name, _currentScope_, item, Replicas[i]);

				var fs = Replicas[i].FileSystem;
				fs.delete(ctx, this);
			}
		}


		void copySafe(IItem[] items, uint index)
		{
			Debug.Assert(items[index] != null);

			var sourceItem = items[index];
			var sourceType = sourceItem.Type;
			var name = sourceItem.Name;

			foreach (var i in items.unsignedIndices())
			{
				if (i == index)
					continue;

				var targetItem_ = items[i];

				// if we have to overwrite stuff from a different type, we have first to 
				// delete it.

				// Otherwise we let the type specific implementation decide (it could benefit in 
				// overwriting it!)

				if (targetItem_ != null && targetItem_.Type != sourceType)
				{
					var targetFS = Replicas[i].FileSystem;
					
					var targetDelete = new ItemContext(name, _currentScope_, targetItem_, Replicas[i]);

					targetFS.delete(targetDelete, this);

					// target item is clear, copy Safe should not see it and assume that there
					// is not file / folder at the target location yet!
					targetItem_ = null;
				}

				var source = new ItemContext(name, _currentScope_, sourceItem, Replicas[index]);
				var target = new ItemContext(name, _currentScope_, targetItem_ /* may be null */, Replicas[i]);

				_types[sourceType.index()].copySafe(source, target, this);
			}
		}

		public IItem merge(IItem[] items)
		{
			Debug.Assert(items.Length == 2 && items[0] != null && items[1] != null);
			if (items[0].Type != items[1].Type)
				return null; // can not merge different behavior items.

			var item1 = new ItemContext(
				items[0].Name,
				_currentScope_,
				items[0],
				Replicas[0]);
			
			var item2 = new ItemContext(
				items[1].Name,
				_currentScope_,
				items[1],
				Replicas[1]);

			return _types[items[0].Type.index()].merge(item1, item2, this);
		}

		#endregion

		#region Scope Enter / Exit (support for IFileSystemScope)

		IDisposable enter(IScope scope)
		{
			IList<IDisposable> disps = new List<IDisposable>();

			foreach (var replica in Replicas)
			{
				var fss = replica.FileSystem;
				if (fss == null)
					continue;

				var entered_ = fss.tryEnter(scope);
				if (entered_ != null)
					disps.Add(entered_);
			}

			return new DisposeAction(() =>
				{
					foreach (var d in disps.Reverse())
					{
						d.Dispose();
					}
				});
		}

		#endregion
	}
}
