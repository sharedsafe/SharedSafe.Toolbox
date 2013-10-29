using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox;
using Toolbox.Collections;

namespace DomainModeling.Detail
{
	sealed class ObjectDistributor : IObjectDistributor
	{
		readonly IHandleEventTransactions[] _transactionHandlers;
		readonly Dictionary<Type, List<Action<IDomainObject>>> _eventHandlers;

		public ObjectDistributor(IEnumerable<object> eventTargets)
		{
			// need to run the transaction handlers in reversed order, so that post-transaction 
			// notification handler that propagate downwards the dependency tree always 
			// run into open transactions.

			_transactionHandlers = (
				from t in eventTargets 
				let x = t as IHandleEventTransactions 
				where x != null 
				select x).Reverse().ToArray();

			_eventHandlers = buildObjectHandlers(eventTargets);
		}

		public IObjectDistributionSession beginSession()
		{
			return new ObjectDistributionSession(_transactionHandlers, _eventHandlers);
		}

		sealed class ObjectDistributionSession : IObjectDistributionSession
		{
			readonly IHandleEventTransactions[] _transactionHandlers;
			readonly Dictionary<Type, List<Action<IDomainObject>>> _eventHandlers;
			readonly IEnumerable<IDisposable> _subTransactions;

			public ObjectDistributionSession(
				IHandleEventTransactions[] transactionHandlers, 
				Dictionary<Type, List<Action<IDomainObject>>> eventHandlers)
			{
				_transactionHandlers = transactionHandlers;
				_eventHandlers = eventHandlers;

				_subTransactions = _transactionHandlers.Select(th => th.beginEventTransaction()).ToArray();
			}

			public void Dispose()
			{
				_subTransactions.Reverse().forEach(d => d.Dispose());
			}

			public void distribute(IDomainObject model)
			{
				var t = model.GetType();
				foreach (var dispatcher in _eventHandlers[t])
				{
					dispatcher(model);
				}
			}
		}

		static Dictionary<Type, List<Action<IDomainObject>>> buildObjectHandlers(IEnumerable<object> targets)
		{
			var handlers = new Dictionary<Type, List<Action<IDomainObject>>>();

			foreach (var target in targets)
			{
				foreach (var handler in extractObjectHandlers(target))
				{
					handlers.add(handler.EventType, handler.Target);
				}
			}

			return handlers;
		}

		static IEnumerable<ObjectHandler> extractObjectHandlers(object target)
		{
			var t = target.GetType();
			foreach (var iface in t.GetInterfaces())
			{
				if (!iface.IsGenericType || !(iface.GetGenericTypeDefinition() == typeof (IBootstrapFromDomainObject<>)))
					continue;

				var eventType = iface.GetGenericArguments()[0];
				yield return makeObjectHandler(target, eventType);
			}
		}

		static ObjectHandler makeObjectHandler(object target, Type objectType)
		{
			return new ObjectHandler(objectType, obj => ObjectDispatcher.bootstrap(target, obj));
		}

		struct ObjectHandler
		{
			public ObjectHandler(Type eventType, Action<IDomainObject> target)
			{
				EventType = eventType;
				Target = target;
			}

			public readonly Type EventType;
			public readonly Action<IDomainObject> Target;
		}
	}
}
