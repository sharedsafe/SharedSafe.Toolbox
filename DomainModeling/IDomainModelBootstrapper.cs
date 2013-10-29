using System;

namespace DomainModeling
{
	public interface IDomainModelBootstrapper
	{
		void bootstrapModel(Action<IDomainObject> dispatch);
	}

	public static class IDomainModelBootstrapperExtensions
	{
		public static void bootstrap(this IDomainModelBootstrapper _, Action<IEventTransaction> transactionDispatcher)
		{
			_.bootstrapModel(o => transactionDispatcher(o.bootstrap().toTransaction()));
		}
	}
}