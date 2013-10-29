/**
	A typed, thread local context stack.
**/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox
{
	public static class Context
	{
		public static IDisposable push<TypeT>(TypeT value)
		{
			return Context<TypeT>.push(value);
		}

		public static IDisposable pushAndOwn<TypeT>(TypeT value)
		{
			return Context<TypeT>.push(value, dispose);
		}

		static void dispose<TypeT>(TypeT t)
		{
			var disp = t as IDisposable;
			if (disp != null)
				disp.Dispose();
		}
	};

	public static class Context<TypeT>
	{
		public static IDisposable push(TypeT value)
		{
			return push(value, null);
		}

		public static IDisposable push(TypeT value, Action<TypeT> popped)
		{
			return Stack.push(value, popped);
		}

		public static TypeT Current
		{
			get 
			{
				return Stack.Current;
			}
		}

		public static TypeT CurrentOrDefault
		{
			get 
			{
				return Stack.CurrentOrDefault;
			}
		}

		public static TypeT Top
		{
			get { return Stack.Top; }
		}

		public static TypeT TopOrDefault
		{
			get { return Stack.TopOrDefault; }
		}

		public static bool Available
		{ get { return Stack.Available; } }

		static ContextStack Stack
		{
			get 
			{
				return StackInternal ?? (StackInternal = new ContextStack());
			}
		}
		
		[ThreadStatic]
		static ContextStack StackInternal;

		sealed class ContextStack
		{
			struct Entry
			{
				public TypeT Value;
				public Action<TypeT> OnPopped;
			};

			readonly Stack<Entry> Stack = new Stack<Entry>();
			readonly IDisposable PopAction;

			public ContextStack()
			{
				PopAction = new DisposeAction(pop);
			}

			public IDisposable push(TypeT value, Action<TypeT> onPopped)
			{
				Stack.Push(new Entry { Value = value, OnPopped = onPopped });
				return PopAction;
			}

			void pop()
			{
				if (Stack.Count == 0)
					throw new InternalError("Context stack of {0} is empty, but pop() was called.".format(typeof(TypeT).Name));

				Entry e = Stack.Pop();
				if (e.OnPopped != null)
					e.OnPopped(e.Value);
			}

			public TypeT Current
			{
				get
				{
					if (Stack.Count == 0)
						throw new InternalError("Context of {0} is not available.".format(typeof(TypeT).Name));

					return Stack.Peek().Value;
				}
			}

			public TypeT CurrentOrDefault
			{
				get
				{
					return Stack.Count == 0 ? default(TypeT) : Stack.Peek().Value;
				}
			}

			public TypeT Top
			{
				get
				{
					if (Stack.Count == 0)
						throw new InternalError("Top of context {0} is not available".format(typeof(TypeT).Name));

					return Stack.First().Value;
				}
			}

			public TypeT TopOrDefault
			{
				get { return Stack.Count == 0 ? default(TypeT) : Stack.First().Value; }
			}

			public bool Available
			{ get { return Stack.Count != 0; } }
		}
	}
}
