using System;
using Newtonsoft.Json;

namespace RootSE.Provider
{
	sealed class NewtonsoftJsonValueSerializer : IValueSerializer
	{
		public string serialize(object instance)
		{
			return JsonConvert.SerializeObject(instance);
		}

		public object deserialize(string serialized, Type type)
		{
			return JsonConvert.DeserializeObject(serialized, type);
		}
	}
}
