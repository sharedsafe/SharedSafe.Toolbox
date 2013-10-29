using System;
using Toolbox;

namespace RootSE.ORM
{
	public static class RepositoryExtensions
	{
		public static InstanceT get<InstanceT>(this Repository<InstanceT> repository, object primaryKey)
			where InstanceT : class
		{
			var instance = repository.tryGet(primaryKey);
			if (instance == null)
				throw new RepositoryException<InstanceT>("failed to get object by primary key {0}".format(primaryKey));
			return instance;
		}

		public static object get(this IRepository repository, object primaryKey)
		{
			var instance = repository.tryGet(primaryKey);
			if (instance == null)
				throw new RepositoryException("failed to get object by primary key {0}".format(primaryKey));

			return instance;
		}

		public static void transact(this IRepository repository, Action action)
		{
			using (var t = repository.beginTransaction())
			{
				action();
				t.commit();
			}
		}

		public static T transact<T>(this IRepository repository, Func<T> f)
		{

			using (var t = repository.beginTransaction())
			{
				T tmp = f();
				t.commit();
				return tmp;
			}
		}
	}



	public static class RepositoryAdvancedExtensions
	{
		public static void modify<InstanceT>(this Repository<InstanceT> repository, object primaryKey, Action<InstanceT> modifier)
		where InstanceT : class
		{
			var instance = repository.get(primaryKey);
			modifier(instance);
			repository.update(instance);
		}
	}
}