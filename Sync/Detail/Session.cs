using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Sync.Detail
{
	sealed class Session : ISession
	{
		/// The original knowledge. I.e. the file system state that contains all
		/// previously synchronized items.
		
		readonly IKnowledge _original;
		readonly IReplica[] _replicas;

		readonly IKnowledge[] _knowledges;
		readonly IDirtyPath[] _dirtyPaths;

		readonly List<IEvent> _events = new List<IEvent>();

		public IEnumerable<IEvent> Events { get { return _events; } }

			/**
			Create a new synchronization session.

			Pass null to original if this is the initial synchronization session of the replicas.
		**/

		public Session(IKnowledge original, IEnumerable<IReplica> replicas)
			: this(original, replicas.ToArray())
		{
		}

		public Session(IKnowledge original, params IReplica[] replicas)
		{
			_replicas = replicas;

			var num = _replicas.Length;

			if (num != 2)
				throw new Exception("Sync frame does not only support to synchronize 2 replicas by now");

			_original = original;

			if (replicas[0].Options != replicas[1].Options)
					throw new Exception("Replica options differ from original knowledge options");

			_knowledges = new IKnowledge[num];
			_dirtyPaths = new IDirtyPath[num];
		}

		public SyncOptions Options
		{
			get { return _replicas[0].Options; }
		}

		/**
			Detect changes of both replicas and store the result in the session.

			todo: We may be able to detect changes in parallel.

			This process scans files and directories but does not open them. It is not
			fault tolerant, if directories are moved while the detection is running it 
			may fail and throw an exception, in this case it must be run again before
			reconcile() may be called.
		**/

		public void scanAndDetectChanges()
		{
			try
			{
				foreach (var i in _replicas.indices())
				{
					var knowledge = _replicas[i].scan();
					var diff = _original.compare(knowledge, Options);

					_knowledges[i] = knowledge;

#if DEBUG
					var dmp = diff.dump();
					if (dmp.Any())
					{
						Log.I("Diff of " + _replicas[i]);
						foreach (var str in diff.dump())
							Log.D(str);
					}
#endif

					_dirtyPaths[i] = ((IDiffCollector)diff).createDirtyPath();
				}
			}
			catch (Exception)
			{
				_knowledges.clear();
				_dirtyPaths.clear();
				throw;
			}
		}

		public bool AnyChanges
		{
			get
			{
				return _dirtyPaths.Any(dirtyPaths => dirtyPaths.Nested.Count != 0);
			}
		}

		/**
			Reconcile changes and return the new "original" knowledge that shall be passed to the next
			synchronization session.

			Reconcile is fault tolerant and tries to continue if desintegrities are happening.
		**/

		public IKnowledge reconcile()
		{
			prepareReconcilation();

			var reconciler = new Reconciler(new Relocator(_replicas));

			// todo: remove Knowledge, I think we don't need identities for file systems, they
			// are always bound in some way to replicas (we see that later, the relocator at least needs to be bound
			// to and so knows where and how the replicas are located.

			var knowledge = new Knowledge(reconciler.reconcile(_knowledges, _dirtyPaths));
			_events.AddRange(reconciler.Events);
			return knowledge;
		}

		public ISyncItem virtualReconcile()
		{
			prepareReconcilation();

			// note: as it seems we are not dependent on replicas anymore
			// (virtual session?)
			var reconciler = new VirtualReconciler();
			var res = reconciler.reconcile(_knowledges, _dirtyPaths);
			_events.AddRange(reconciler.Events);
			return res;
		}

		void prepareReconcilation()
		{
			_events.Clear();

			// check if dirty paths are set.

			foreach (var i in _knowledges.indices())
				if (_knowledges[i] == null || _dirtyPaths[i] == null)
					throw new Exception("DetectChanges did not completed fully");
		}

		/**
			Reconcile one side of the sync item tree.

			The index specifies the side to reconcile.
		**/

		public void reconcile(ISyncItem item, uint target, IReconcilePolicy policy)
		{
			prepareReconcilation();

			var relocator = new Relocator(_replicas);
			var reconciler = new PartialReconciler(relocator);

			reconciler.reconcile(item, target, policy);

			_events.AddRange(reconciler.Events);
		}
	}
}
