using System;

namespace Toolbox.IoC
{
	interface IFactory
	{
		object tryResolveFor(Type typeThatRequestsFactory);
	}

	sealed class Factory<GeneratedT> : IFactory
	{
		readonly Type _typesAllowedToReceiveThatFactory;
		readonly Func<GeneratedT> _generator;

		public Factory(Type typesAllowedToReceiveThatFactory, Func<GeneratedT> generator)
		{
			_typesAllowedToReceiveThatFactory = typesAllowedToReceiveThatFactory;
			_generator = generator;
		}

		public object tryResolveFor(Type typeThatRequestsFactory)
		{
			return !_typesAllowedToReceiveThatFactory.IsAssignableFrom(typeThatRequestsFactory) ? null : _generator;
		}
	}
}
