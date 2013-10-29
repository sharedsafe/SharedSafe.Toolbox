using System;
using System.Collections.Generic;
using Toolbox.Sync.Detail;

namespace Toolbox.Sync
{
	// todo: we may need live reporting :(

	public interface ISession
	{
		SyncOptions Options { get; }

		void scanAndDetectChanges();
		bool AnyChanges { get; }

		IKnowledge reconcile();

		/// todo: may put this in a kind of a virtual session
		ISyncItem virtualReconcile();

		/// Partially reconcile beginning at root the sided updates specified by index!
		
		/// @param root The root of the reconcilation
		/// @param index The horizontal (item) index to resolve in this partial reconcilation
		/// @param confirm The confirmation method
		
		void reconcile(ISyncItem root, uint index, IReconcilePolicy policy);

		/// The events of the last reconcilation!
		IEnumerable<IEvent> Events { get; }
	}

	public static class SessionExtensions
	{
		public static void reconcile(this ISession session, ISyncItem root, uint index)
		{
			session.reconcile(root, index, DefaultReconcilePolicy.Instance);
		}
	}
}
