using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RootSE
{
	public interface IStorage : IDisposable
	{
		IDisposable beginSession();
		IDisposable beginTransaction();
		void commit();

		void store(object obj);

		IEnumerable<object> queryAll(Type t);
		IEnumerable<DocumentT> queryByKey<DocumentT, KeyT>(Expression<Func<DocumentT, KeyT>> keyMember, string keyValue);


		// define a relation.
		IRelation<FromT, ToT> relation<FromT, ToT>(string name);
	}
}
