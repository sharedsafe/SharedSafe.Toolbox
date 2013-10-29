/**
	A static integration context is used to provide static (context free) integration a defined
	context in which they run. 

	A integration context is a stack of RuntimeIntegrators.

	note: Using this class is not thread safe. But we have to live with it right now. Usability comes first.
	note: I may be later solved by thread local variables, but then an IntegrationSpace must be bound to a thread 
	(which it probably should).
**/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox;

namespace LibG4.Detail
{
	static class GlobalIntegrationContext
	{
		static readonly Stack<Integration> _stack = new Stack<Integration>();
		static readonly IDisposable _pop = new DisposeAction(pop);

		/**
			push a new context.

			always use using() to access push()
		**/

		public static IDisposable push(Integration integrator)
		{
			_stack.Push(integrator);
			return _pop;
		}

		/**
			pop a context.
		**/

		static void pop()
		{
			Debug.Assert(_stack.Count != 0);
			_stack.Pop();
		}

		/// Return the current context

		public static Integration Context
		{
			get 
			{
				if (_stack.Count == 0)
					throw new Exception("Missing Current integration context, please check if there are any context free event handlers in the call stack!");

				return _stack.Peek(); 
			}
		}
	
	}
}
