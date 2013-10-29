using System;

namespace LibG4.Detail
{
	struct Integrator
	{
		public readonly Type ModelType;
		public readonly Type EnvironmentType;
		public readonly Func<IntegrationSpace, object, object, Integration> CreateInstance;

		public Integrator(Type modelType, Type environmentType, Func<IntegrationSpace, object, object, Integration> createInstance)
		{
			ModelType = modelType;
			EnvironmentType = environmentType;
			CreateInstance = createInstance;
		}

		public string Name
		{ get { return ModelType.Name; } }
	}
}
