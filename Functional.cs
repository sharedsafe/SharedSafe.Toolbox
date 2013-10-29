using System;

namespace Toolbox
{
	public static class Functional
	{
		public static Action chain(this Action action, Action newAction)
		{
			if (action == null)
				return newAction;

			if (newAction == null)
				return action;

			return () =>
				{
					action();
					newAction();
				};
		}

		public static Action<ArgT> chain<ArgT>(this Action<ArgT> action, Action<ArgT> newAction)
		{
			if (action == null)
				return newAction;

			if (newAction == null)
				return action;

			return a =>
			{
				action(a);
				newAction(a);
			};
		}

		public static Action<ArgT, Arg2T> chain<ArgT, Arg2T>(this Action<ArgT, Arg2T> action, Action<ArgT, Arg2T> newAction)
		{
			if (action == null)
				return newAction;

			if (newAction == null)
				return action;

			return (a, a2) =>
			{
				action(a, a2);
				newAction(a, a2);
			};
		}
	}
}
