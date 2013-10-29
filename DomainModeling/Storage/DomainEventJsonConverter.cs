using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DomainModeling.Storage
{
	sealed class DomainEventJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var de = (DomainEvent) value;

			Debug.Assert(de.Event.GetType().Namespace == DomainEventRegistry.Current.Namespace);

			var o = new JObject(new JProperty(de.Event.GetType().Name, JObject.FromObject(de.Event)));
			serializer.Serialize(writer, o);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var o = (JObject)serializer.Deserialize(reader);
			var property = o.Properties().Single();
			var type = DomainEventRegistry.Current.lookup(property.Name);

			return new DomainEvent {Event = (IDomainEvent)serializer.Deserialize(new JTokenReader(property.Value), type)};
		}
	}
}
