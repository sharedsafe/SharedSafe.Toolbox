using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox;
using Toolbox.Collections;

namespace DomainModeling.Detail
{
	sealed class EventDistributor : IEventDistributor
	{
		readonly IHandleEventTransactions[] _transactionHandlers;
		readonly Dictionary<Type, List<Action<IDomainEvent>>> _eventHandlers;

		public EventDistributor(IEnumerable<object> eventTargets)
		{
			// need to run the transaction handlers in reversed order, so that post-transaction 
			// notification handler that propagate downwards the dependency tree always 
			// run into open transactions.

			_transactionHandlers = (
				from t in eventTargets 
				let x = t as IHandleEventTransactions 
				where x != null 
				select x).Reverse().ToArray();

			_eventHandlers = buildEventHandlers(eventTargets);
		}

		public IEventDistributionSession beginSession()
		{
			return new EventDistributionSession(_transactionHandlers, _eventHandlers);
		}

		sealed class EventDistributionSession : IEventDistributionSession
		{
			readonly IHandleEventTransactions[] _transactionHandlers;
			readonly Dictionary<Type, List<Action<IDomainEvent>>> _eventHandlers;
			readonly IEnumerable<IDisposable> _subTransactions;

			public EventDistributionSession(IHandleEventTransactions[] transactionHandlers, 
				Dictionary<Type, List<Action<IDomainEvent>>> eventHandlers)
			{
				_transactionHandlers = transactionHandlers;
				_eventHandlers = eventHandlers;

				_subTransactions = _transactionHandlers.Select(th => th.beginEventTransaction()).ToArray();
			}

			public void Dispose()
			{
				_subTransactions.Reverse().forEach(d => d.Dispose());
			}

			public void distribute(IDomainEvent ev)
			{
				var t = ev.GetType();
				foreach (var dispatcher in _eventHandlers[t])
				{
					dispatcher(ev);
				}
			}
		}

		static Dictionary<Type, List<Action<IDomainEvent>>> buildEventHandlers(IEnumerable<object> targets)
		{
			var handlers = new Dictionary<Type, List<Action<IDomainEvent>>>();

			foreach (var target in targets)
			{
				foreach (var handler in extractEventHandlers(target))
				{
					handlers.add(handler.EventType, handler.Target);
				}
			}

			return handlers;
		}

		static IEnumerable<EventHandler> extractEventHandlers(object target)
		{
			var t = target.GetType();
			foreach (var iface in t.GetInterfaces())
			{
				if (!iface.IsGenericType || !(iface.GetGenericTypeDefinition() == typeof (IHandleEvent<>)))
					continue;

				var eventType = iface.GetGenericArguments()[0];
				yield return makeEventHandler(target, eventType);
			}
		}

		static EventHandler makeEventHandler(object target, Type eventType)
		{
			return new EventHandler(eventType, ev => EventDispatcher.handle(target, ev));
		}

		struct EventHandler
		{
			public EventHandler(Type eventType, Action<IDomainEvent> target)
			{
				EventType = eventType;
				Target = target;
			}

			public readonly Type EventType;
			public readonly Action<IDomainEvent> Target;
		}
	}
}
