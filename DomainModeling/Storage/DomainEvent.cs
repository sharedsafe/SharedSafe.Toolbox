using System.Reflection;
using Newtonsoft.Json;

namespace DomainModeling.Storage
{
	[Obfuscation, JsonConverter(typeof(DomainEventJsonConverter))]
	sealed class DomainEvent
	{
		public IDomainEvent Event;
	}
}