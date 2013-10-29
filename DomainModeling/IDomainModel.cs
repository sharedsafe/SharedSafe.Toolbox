using System.Collections.Generic;
using DomainModeling.Meta;

namespace DomainModeling
{
	public interface IDomainModel
	{
		IEnumerable<MetaType> MetaTypes { get; }
	}
}
