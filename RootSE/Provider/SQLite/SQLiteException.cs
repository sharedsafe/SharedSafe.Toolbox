using System;

namespace RootSE.Provider.SQLite
{
	public class SQLiteException : Exception
	{
		public SQLiteException(string message) :
			base(message)
		{

		}
	}
}