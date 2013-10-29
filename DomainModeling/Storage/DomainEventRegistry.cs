using System;
using System.Collections.Generic;
using System.Reflection;
using Toolbox;

namespace DomainModeling.Storage
{
	sealed class DomainEventRegistry
	{
		readonly IDomainEventsSpecification _eventStoreMeta;
		readonly Dictionary<string, Type> _typeMap;

		public string Namespace
		{
			get { return _eventStoreMeta.TypeOfFirstEvent.Namespace; }
		}

		public DomainEventRegistry(IDomainEventsSpecification eventStoreMeta)
		{
			_eventStoreMeta = eventStoreMeta;
			_typeMap = discoverEvents(_eventStoreMeta.TypeOfFirstEvent);
		}
		
		Dictionary<string, Type> discoverEvents(Type firstEventType)
		{
			var typeMap = new Dictionary<string, Type>();
			var firstEventNamespace = firstEventType.Namespace;

			foreach (var t in Assembly.GetAssembly(firstEventType).GetTypes())
			{
				if (t.Namespace == firstEventNamespace && typeof(IDomainEvent).IsAssignableFrom(t))
					typeMap.Add(t.Name, t);
			}

			return typeMap;
		}

		public Type lookup(string str)
		{
			return _typeMap[str];
		}

		public IDisposable makeCurrent()
		{
			return Context.push(this);
		}

		public static DomainEventRegistry Current
		{
			get { return Context<DomainEventRegistry>.Current; }
		}
	}
}
