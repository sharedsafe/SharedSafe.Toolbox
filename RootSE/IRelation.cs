using System.Collections.Generic;

namespace RootSE
{
	public interface IRelation<FromT, ToT>
	{
		string Name { get; }

		IEnumerable<ToT> queryForward(FromT from);
		IEnumerable<FromT> queryBackward(ToT to);
	}
}
