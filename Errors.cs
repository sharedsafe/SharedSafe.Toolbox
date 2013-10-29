/**
	Useful exception types.
**/

using System;

namespace Toolbox
{
	public class InternalError : Exception
	{
		/**
			Note: the objecs passed are just for later reference, and not 
			used for formatting the description, use .format() instead.

			Captured data should be specified from "near" to "far" as seen 
			from the problem situation.
		**/


		public InternalError(string description, params object[] capturedData)
			: base("Internal Error: " + description)
		{
			CapturedData = capturedData;
		}

		public InternalError(Exception inner, string description)
			: base("Internal Error: " + description + ": " + inner.Message, inner)
		{
			CapturedData = Array<object>.Empty;
		}

		public object[] CapturedData { get; private set; }
	}
}