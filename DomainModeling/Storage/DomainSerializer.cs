using System;
using Konstruktor;
using Newtonsoft.Json;
using RootSE.Provider;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class DomainSerializer : IValueSerializer
	{
		readonly DomainEventRegistry _domainEventRegistry;

		public DomainSerializer(DomainEventRegistry domainEventRegistry)
		{
			_domainEventRegistry = domainEventRegistry;
		}

		public string serialize(object instance)
		{
			return JsonConvert.SerializeObject(instance);
		}

		public object deserialize(string serialized, Type type)
		{
			using (_domainEventRegistry.makeCurrent())
			{
				return JsonConvert.DeserializeObject(serialized, type);
			}
		}
	}
}
