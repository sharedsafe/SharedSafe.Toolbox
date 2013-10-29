using System;
using System.Collections.Generic;

namespace DomainModeling.Detail
{
	static class ObjectDispatcher
	{
		public static void bootstrap(object target, IDomainObject obj)
		{
			var dispatcher = resolveFor(obj.GetType());
			dispatcher.bootstrap(target, obj);
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
			public abstract void bootstrap(object target, IDomainObject obj);
		}

		sealed class Dispatcher<ObjectT> : Dispatcher
			where ObjectT : IDomainObject
		{
			public override void bootstrap(object target, IDomainObject obj)
			{
				((IBootstrapFromDomainObject<ObjectT>)target).bootstrap((ObjectT)obj);
			}
		}
	}
}
