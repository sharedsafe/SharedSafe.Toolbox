using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	sealed class SyncItem : ISyncItem
	{
		Dictionary<string, ISyncItem> _nested_;

		public SyncItem(IItem[] items, SyncItemState state, SyncItemSubState subState)
			: this(nameFromItems(items), items, state, subState)
		{
		}

		public SyncItem(IItem[] items, SyncItemSubState subState, uint index)
			: this(nameFromItems(items), items, SyncItemState.Reconcile, subState, index)
		{
		}

		public SyncItem(string name, IItem[] items, SyncItemState state, SyncItemSubState subState)
			: this (name, items, state, subState, null)
		{
			Debug.Assert(state != SyncItemState.Reconcile);
		}

		SyncItem(string name, IItem[] items, SyncItemState state, SyncItemSubState subState, uint? index)
		{
			Debug.Assert(name != null && items != null);
			Name = name;
			Items = items;
			State = state;
			SubState = subState;
			ReconcileIndex = index;
		}

		#region ISyncItem Members

		public string Name { get; private set; }

		public IItem[] Items { get; private set; }


		public IEnumerable<ISyncItem> Nested
		{
			get { return _nested_ != null ? _nested_.Values : Enumerable.Empty<ISyncItem>(); }
		}

		public void add(ISyncItem item)
		{
			if (_nested_ == null)
				_nested_ = new Dictionary<string, ISyncItem>();
			_nested_.Add(item.Name, item);
		}

		public SyncItemState State { get; private set; }
		public SyncItemSubState SubState { get; private set; }
		public SyncItemSubState SubStateReconciled { get; private set; }
		public uint? ReconcileIndex { get; private set; }
		public Exception ReconcilationError_ { get; private set; }

		public void markMergeConflict()
		{
			if (State != SyncItemState.NeedsMerge || SubState != SyncItemSubState.NeedsMerge)
				throw this.error("Can not tag a sync item with conflict state, when it is not in Merge right now");

			State = SyncItemState.Conflict;
			SubState = SyncItemSubState.ConflictUpdateUpdate;
		}

		public void setReconcilationError(Exception e)
		{
			if (!State.isOneOf(SyncItemState.Reconcile, SyncItemState.Reconciling))
				throw this.error("Can't set a reconcilation error, when this item is not scheduled for reconcilation");

			ReconcilationError_ = e;
		}

		public void merged(IItem item)
		{
			if (State != SyncItemState.NeedsMerge || SubState != SyncItemSubState.NeedsMerge)
				throw this.error("Sync item not in merge state, but mergeIgnore() called");

			foreach (var i in Items.indices())
				Items[i] = item;

			State = SyncItemState.InSync;
			SubState = SyncItemSubState.Merged;
		}

		public void reconciling()
		{
			if (State != SyncItemState.Reconcile)
				throw this.error("Can not tag a sync item to be reconciling, when it is not about to be reconciled");

			State = SyncItemState.Reconciling;
		}

		public void reconciled()
		{
			if (State != SyncItemState.Reconcile && State != SyncItemState.Reconciling)
				throw this.error("Can not tag a sync item to be reconciled, when it is not about to be reconciled");

			Debug.Assert(ReconcileIndex != null);
			var dominant_ = Items[ReconcileIndex.Value];

			// this is required for tryMakeItemTree for exmple
			foreach (var i in Items.indices())
				Items[i] = dominant_;

			State = SyncItemState.InSync;
			SubStateReconciled = SubState;
			SubState = SyncItemSubState.Reconciled;
		}

		public void resolveConflict(uint dominant)
		{
			if (State != SyncItemState.Conflict)
				throw this.error("Can not resolve non-conflicting sync item.");

			if (dominant < 0 || dominant >= Items.Length)
				throw this.error("Resolve index out of range");

			switch (SubState)
			{
				case SyncItemSubState.ConflictUpdateDelete:
					SubState = 
						Items[dominant] == null 
						? SyncItemSubState.ReconcileDelete 
						: SyncItemSubState.ReconcileCreate;
					break;

				case SyncItemSubState.ConflictUpdateUpdate:
					SubState = SyncItemSubState.ReconcileUpdate;
					break;

				default:
					throw this.error("Internal error: unexpected conflict substate");
			}

			State = SyncItemState.Reconcile;
			ReconcileIndex = dominant;
		}

		public ISyncItem tryLocate(string name)
		{
			if (_nested_ == null)
				return null;

			ISyncItem r;
			_nested_.TryGetValue(name, out r);
			return r;
		}

		#endregion

		#region Helper

		static string nameFromItems(IEnumerable<IItem> items)
		{
			var x = from i in items where i != null select i;
			var str = x.FirstOrDefault();
			if (str == null)
				throw new ArgumentException("Items must be set when name is not given");
			return str.Name;
		}

		#endregion

		public override string ToString()
		{
			var str = State + "," + SubState;
			if (ReconcileIndex.HasValue)
				str += "," + ReconcileIndex.Value;

			str += " " + Name;

			return str;
		}
	}
}
