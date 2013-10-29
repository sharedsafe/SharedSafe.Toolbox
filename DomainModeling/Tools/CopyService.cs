using System.Collections.Generic;
using DomainModeling.Storage;
using Konstruktor;

namespace DomainModeling.Tools
{
	[DefaultImplementation]
	sealed class CopyService : ICopyService
	{
		readonly DomainModelRegistry _registry;

		public CopyService(DomainModelRegistry registry)
		{
			_registry = registry;
		}

	
		public IIndividualization beginCopySession()
		{
			return new Individualization(_registry);
		}
	}
}
