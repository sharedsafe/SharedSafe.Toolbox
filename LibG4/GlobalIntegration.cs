/**
	Global integration extensions.
**/

using LibG4.Detail;

namespace LibG4
{
	public static class GlobalIntegration
	{
		/**
			Integration of Collection<ModelT>
		**/

		public static void integrate<ModelT, EnvironmentT>(this Collection<ModelT> collection, EnvironmentT environment)
		{
			var rc = GlobalIntegrationContext.Context;
			rc.integrate(collection, environment);
		}
	}
}
