using System;


namespace Toolbox
{
	/**
		Mis - usings :)
	**/

	public static class Scoped
	{
		public static IDisposable actions(Action a, Action b)
		{
			a();
			return new DisposeAction(b);
		}
	}
}
