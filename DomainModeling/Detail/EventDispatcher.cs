using System;
using System.Collections.Generic;

namespace DomainModeling.Detail
{
	static class EventDispatcher
	{
		public static IEnumerable<IDomainEvent> transform(object target, IDomainEvent ev)
		{
			var dispatcher = resolveFor(ev.GetType());
			return dispatcher.transform(target, ev);
		}

		public static void handle(object target, IDomainEvent ev)
		{
			var dispatcher = resolveFor(ev.GetType());
			dispatcher.handle(target, ev);
		}

		static Dispatcher resolveFor(Type type)
		{
			if (Dispatchers == null)
				Dispatchers = new Dictionary<Type, Dispatcher>();

			Dispatcher dispatcher;
			if (Dispatchers.TryGetValue(type, out dispatcher))
				return dispatcher;

			var typeToInstantiate = typeof (Dispatcher<>).MakeGenericType(type);
			dispatcher = (Dispatcher)Activator.CreateInstance(typeToInstantiate);
			Dispatchers.Add(type, dispatcher);
			return dispatcher;
		}

		[ThreadStatic] static Dictionary<Type, Dispatcher> Dispatchers;

		abstract class Dispatcher
		{
			public abstract IEnumerable<IDomainEvent> transform(object target, IDomainEvent ev);
			public abstract void handle(object target, IDomainEvent ev);
		}

		sealed class Dispatcher<EventT> : Dispatcher
			where EventT : IDomainEvent
		{
			public override IEnumerable<IDomainEvent> transform(object target, IDomainEvent ev)
			{
				return ((ITransformEvent<EventT>)target).transform((EventT)ev);
			}

			public override void handle(object target, IDomainEvent ev)
			{
				((IHandleEvent<EventT>)target).handle((EventT)ev);
			}
		}
	}
}
