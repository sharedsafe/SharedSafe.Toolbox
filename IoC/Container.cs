using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toolbox.IoC
{
	public sealed class Container
	{
		readonly TypeMap _typeMap = new TypeMap(null);
		readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
		// a map from the type generated to a Factory
		readonly Dictionary<Type, IFactory> _factories = new Dictionary<Type, IFactory>();


		#region DSL

		public Container map<FromT, ToT>()
		{
			_typeMap.add(typeof(FromT), typeof(ToT));
			return this;
		}

		public Container instance<TypeT>(TypeT t)
		{
			_instances.Add(typeof(TypeT), t);
			return this;
		}

		public Container factory<CompT, GeneratedT>(Func<GeneratedT> f)
		{
			_factories.Add(typeof (GeneratedT), new Factory<GeneratedT>(typeof(CompT), f));
			return this;
		}

		#endregion

		#region Context Support

		public IDisposable push()
		{
			return Context.push(this);
		}

		public static Container Current
		{
			get { return Context<Container>.Current; }
		}

		#endregion

		#region Create a new object

		public TypeT create<TypeT>(params object[] predefined)
		{
			Session session = createSession(predefined);
			return session.create<TypeT>();
		}

		public object invoke(MethodInfo method, params object[] predefined)
		{
			Session session = createSession(predefined);
			return session.invoke(method);
		}

		Session createSession(IEnumerable<object> predefined)
		{
			var instances = from p in predefined select new KeyValuePair<Type, object>(p.GetType(), p);
			var dict = _instances.Concat(instances);

			return new Session(this, null, dict);
		}

		#endregion

		public Type resolveType(Type t)
		{
			return _typeMap.resolveOrSame(t);
		}

		internal bool tryResolveFactory(Type neededType, out IFactory factory)
		{
			return _factories.TryGetValue(neededType, out factory);
		}
	}
}
