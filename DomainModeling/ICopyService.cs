using System;
using System.Collections.Generic;

namespace DomainModeling
{
	public interface ICopyService
	{
		IIndividualization beginCopySession();
	}

	public interface IIndividualization : IDisposable
	{
		IEnumerable<IDomainObject> individualize(IEnumerable<IDomainObject> sources);
		IEnumerable<DomainT> individualize<DomainT>(IEnumerable<DomainT> sources)
			where DomainT : IDomainObject;
	}
}