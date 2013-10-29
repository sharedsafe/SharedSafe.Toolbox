using System;
using System.Reflection;
using LibG4.Detail;
using Toolbox;
using Toolbox.Evolution;
using Toolbox.Meta;
using System.Collections.Generic;

namespace LibG4
{
	public static class G4
	{
#if false
		static class InterfaceMeta<IT>
		{
			public static readonly string TypeSpace = typeof(IT).getAttribute<TypeSpaceAttribute>().Space;
		}
#endif

		/**
			Create a live transaction. The live transaction is scheduled in the current
			scope to be run the first time when the scope ends.
		**/

		public static Transaction live(Action action)
		{
			var transaction = new Transaction(action);
			transaction.schedule();
			return transaction;

		}

		public static void touch<T>(T v)
		{
		}

		public static void stopTracking()
		{
			var transaction = TransactionManager.CurrentTransaction;
			if (transaction == null)
				throw new Exception("No current transaction: stopTracking() can not be called");

			transaction.stopTracking();
		}

		/**
			Attach some live disposable object that needs to be disposed when
			the current transaction or Integration is disposed.


		**/

		public static void attach(IDisposable live)
		{
			var transaction = TransactionManager.CurrentTransaction;
			if (transaction == null)
				throw new Exception("Cannot attach {0}, no current transaction or integration.".format(live));
				
			transaction.attach(live);
		}

		// todo: extension method?

		public static void bind(IDisposable obj)
		{
			var integration = GlobalIntegrationContext.Context;
			if (integration == null)
				throw new InternalError("Cannot bind object, no current integration", obj);

			integration.destructor(obj.Dispose);
		}

		#region Global Integration

		/// Permanently maintain the provided action by rerunning it when 
		/// accessed properties are changed.

		public static void maintain<KeyT>(Action action)
		{
			GlobalIntegrationContext.Context.maintain<KeyT>(action);
		}

		/// Remove associated maintenance action.
		/// todo: May call this unmaintain :)

		public static void maintain<KeyT>()
		{
			GlobalIntegrationContext.Context.maintain<KeyT>();
		}

		/// Maintains the action until the current integration is ended.

		public static void maintain(Action action)
		{
			GlobalIntegrationContext.Context.maintain(action);
		}

		/// Update all pending transactions.
		/// todo: try to get rid of the public here, applications may need to call G4.update() at startup :(
		
		public static void update()
		{
			TransactionManager.update();
		}

		/// The current context in a transportable way (this can be used to capture event handlers)
		/// A using() on the returned object enters the context and if the scope ends, leaves the context and
		/// calls G4.update()

		public static Func<IDisposable> Context
		{
			get
			{
				// capture current context
				var ctx = GlobalIntegrationContext.Context;

				return () =>
				{
					var dispose = GlobalIntegrationContext.push(ctx);

					// todo: cache DisposeAction()
					return new DisposeAction(() =>
					{
						// pop context!
						dispose.Dispose();
						// update
						// todo: may update only if the current Context is really null !!!
						update();
					});
				};
			}
		}

		#endregion

		#region Type Resolvements (assembly)

		public static Type resolveImplementationType(string assembly, string name)
		{
			if (InterfaceMap == null)
				InterfaceMap = makeInterfaceMap();

			Type interfaceType;
			if (!InterfaceMap.TryGetValue(Pair.make(assembly, name), out interfaceType))
				return null;

			return resolveImplementationType(interfaceType);
		}

		public static Type resolveImplementationType(Type interfaceType)
		{
			var attr = interfaceType.queryAttribute<ImplementationAttribute>(); 
			if (attr == null)
				throw new InternalError("Unable to resolve interface type {0}, ImplementationAttribute missing".format(interfaceType.Name), interfaceType);

			return TypeBuilderStatic.resolveMixin(attr.Scope, interfaceType.asEnumerable());
		}

		[ThreadStatic]
		static Dictionary<Pair<string, string>, Type> InterfaceMap;

		static Dictionary<Pair<string, string>, Type> makeInterfaceMap()
		{
			var attributes = AppDomain.CurrentDomain.getTypeAttributes<ImplementationAttribute>();
			var dict = new Dictionary<Pair<string, string>, Type>();

			foreach (var attr in attributes)
			{
				var type = attr.First;
				var implementationAttr = attr.Second;
				var implementationNamespace = implementationAttr.Scope.Name;
				dict.Add(Pair.make(implementationNamespace, type.Name), type);
			}
			return dict;
		}

		#endregion
	}

	
}
