namespace Toolbox
{
	public static class Require
	{
		public static InstanceT notNull<InstanceT>(this InstanceT instance, string explain) 
			where InstanceT : class
		{
			if (instance == null)
				throw new InternalError(explain);

			return instance;
		}
	}
}
