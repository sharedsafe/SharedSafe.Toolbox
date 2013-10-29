using System;

namespace RootSE.ORM
{
	[Serializable]
	public class RepositoryException : Exception
	{
		public RepositoryException(string message) 
			: base(message)
		{
		}
	}

	[Serializable]
	public class RepositoryException<InstanceT> : RepositoryException
	{
		public RepositoryException(string message)
			: base(typeof(InstanceT) + " repository: " + message)
		{
		}
	}
}