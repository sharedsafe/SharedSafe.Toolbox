using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootSE.Engine
{
	sealed class Relation<FromT, ToT> : IRelation<FromT, ToT>
	{
		readonly RelationStorage _rs;
		public string Name { get; private set; }
		public string TableName { get; private set; }

		public IEnumerable<ToT> queryForward(FromT from)
		{
			return _rs.queryForward<FromT, ToT>(TableName, from);
		}

		public IEnumerable<FromT> queryBackward(ToT to)
		{
			return _rs.queryBackward<FromT, ToT>(TableName, to);

		}

		public Relation(RelationStorage rs, string name)
		{
			_rs = rs;
			TableName = makeTableName(name);
		}

		static string makeTableName(string name)
		{
			return Conventions.ReservedPrefix + "R" + name + "_" + typeof(FromT).Name + "_" + typeof(ToT).Name;
		}

	}
}
