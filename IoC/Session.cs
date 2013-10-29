using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Toolbox.IoC
{
	/**
		Session information for generating an object graph.
	**/

	sealed class Session
	{
		readonly Container _container;
		readonly Session _parent_;
		readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
		readonly Dictionary<Type, object> _assignableInstancesCache = new Dictionary<Type, object>();
		readonly IDictionary<Type, IFactory> _factories = new Dictionary<Type, IFactory>();

		public Session(Container container, Session _parent_)
			: this(container, _parent_, Enumerable.Empty<KeyValuePair<Type, object>>())
		{

		}
	
		public Session(Container container, Session parent_, IEnumerable<KeyValuePair<Type, object>> instances)
		{
			_container = container;
			_parent_ = parent_;
			_instances = instances.ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		#region Instances

		public bool hasInstanceFor(Type t)
		{
			return _instances.ContainsKey(t) || _parent_ != null && _parent_.hasInstanceFor(t);
		}

		public bool tryGetAssignableInstanceFor(Type t, out object instance)
		{
			return tryGetAssignableInstanceForLocal(t, out instance) 
				|| _parent_ != null && _parent_.tryGetAssignableInstanceFor(t, out instance);
		}

		bool tryGetAssignableInstanceForLocal(Type t, out object instance)
		{
			if (_instances.TryGetValue(t, out instance))
				return true;

			if (_assignableInstancesCache.TryGetValue(t, out instance))
				return true;

			foreach (var inst in _instances)
			{
				if (!t.IsAssignableFrom(inst.Key))
					continue;

				instance = inst.Value;

				_assignableInstancesCache.Add(t, instance);
				return true;
			}

			return false;
		}
	
		
		public void addInstance(Type t, object instance)
		{
			Debug.Assert(!hasInstanceFor(t));
			_instances.Add(t, instance);
		}

		#endregion

		#region Object construction

		public TypeT create<TypeT>()
		{
			return (TypeT)create(_container.resolveType(typeof(TypeT)));
		}

		object create(Type t)
		{
			Debug.Assert(!hasInstanceFor(t));

			if (t.IsInterface)
				throw this.error("Failed to create an instance of interface {0}".format(t));

			var constructors = t.GetConstructors();
			if (constructors.Length != 1)
				throw this.error("Expected one single constructor for {0} to be created.".format(t));

			var constructor = constructors[0];
			IEnumerable<object> args = resolveArguments(t, constructor);

			return constructor.Invoke(args.ToArray());
		}
		#endregion

		#region Method Invocation

		public object invoke(MethodInfo mi)
		{
			if (!mi.IsStatic || !mi.IsPublic)
				throw this.error("Method is not static or not public {0}".format(mi));

			return mi.Invoke(null, resolveArguments(null, mi).ToArray());
		}

		#endregion

		IEnumerable<object> resolveArguments(Type requestor_, MethodBase method)
		{
			var parameters = method.GetParameters();
			var parameterTypes = from p in parameters select p.ParameterType;
			return resolveInstances(requestor_, parameterTypes);
		}

		IEnumerable<object> resolveInstances(Type requestor_, IEnumerable<Type> types)
		{
			return from t in types select resolveInstance(requestor_, t);
		}

		object resolveInstance(Type requestor_, Type tRequested)
		{
			Type t = _container.resolveType(tRequested);

			// factory required?

			if (tRequested.IsGenericType && tRequested.GetGenericTypeDefinition().Equals(typeof(Func<>)))
			{
				if (requestor_ == null)
					throw this.error("A factory method is required to be resolved, by no requestor type is given");
				return resolveFactory(requestor_, tRequested);
			}

			object inst;

			if (!tryGetAssignableInstanceFor(t, out inst))
				addInstance(t, inst = create(t));

			return inst;
		}

		object resolveFactory(Type requestor, Type tRequested)
		{
			var genericArguments = tRequested.GetGenericArguments();
			Debug.Assert(genericArguments.Length == 1);
			var neededType = genericArguments[0];

			IFactory f;
			if (!tryResolveFactory(neededType, out f))
				_factories[neededType] = f = createImplicitFactoryFor(neededType);

			var factoryFunc = f.tryResolveFor(requestor);
			if (factoryFunc == null)
				throw this.error("Type {0} not allowed to receive a factory that generates {1}".format(requestor, neededType));

			return factoryFunc;
		}


		bool tryResolveFactory(Type neededType, out IFactory factory)
		{
			if (_factories.TryGetValue(neededType, out factory) || _parent_ != null && _parent_.tryResolveFactory(neededType, out factory))
				return true;

			return _container.tryResolveFactory(neededType, out factory);
		}

		IFactory createImplicitFactoryFor(Type t)
		{
			var f = typeof (ImplicitFactory<>);
			var gf = f.MakeGenericType(t);
			// want this to be an isolated session
			return (IFactory)Activator.CreateInstance(gf, new Session(_container, this));
		}

		sealed class ImplicitFactory<TypeT> : IFactory
		{
			readonly Session _session;

			public ImplicitFactory(Session session)
			{
				_session = session;
			}

			public object tryResolveFor(Type typeThatRequestsFactory)
			{
				return (Func<TypeT>) (() => _session.create<TypeT>());
			}
		}

	}
}
