using System;
using System.Diagnostics;

namespace Toolbox
{
	/**
		Safe stuff.
	**/

	public static class Safe
	{
		/**
			Call a function and return a default return code if an exception was catched.
		**/


		public static ResultT call<ResultT>(Func<ResultT> a)
		{
			return call(a, default(ResultT));
		}

		public static ResultT call<ResultT>(Func<ResultT> a, ResultT e)
		{
			try
			{
#if DEBUG
				using (Context.push(new SafeContext()))
#endif
					return a();
			}
			catch (Exception ex)
			{
				Log.I(ex.Message);
				return e;
			}
		}

		struct SafeContext { };

		[Conditional("DEBUG")]
		public static void assert()
		{
#if DEBUG
			Debug.Assert(Context<SafeContext>.Available);
#endif
		}

	}
}


