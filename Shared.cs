using System;
using System.Threading;

namespace Toolbox
{
	public interface IShared<T>
		where T : class
	{
		IDisposable acquire(out T instance);
		void use(Action<T> action);
		bool tryUse(Action<T> action);
		bool tryUse(Action<T> action, int timeout);
		V take<V>(Func<T, V> f);
	}

	sealed class Shared<T> : IShared<T>
		where T : class
	{
		readonly T _v;
		
		public Shared(T v)
		{
			_v = v;
		}

		#region IShared<T> Members

		public IDisposable acquire(out T instance)
		{
			var r = new DisposeAction(() => Monitor.Exit(_v));
			Monitor.Enter(_v);
			instance = _v;
			return r;
		}


		public void use(Action<T> action)
		{
			lock (_v)
				action(_v);
		}

		public bool tryUse(Action<T> action)
		{
			if (!Monitor.TryEnter(_v))
				return false;
			try
			{
				action(_v);
			}
			finally
			{
				Monitor.Exit(_v);
			}
	
			return true;
		}

		public bool tryUse(Action<T> action, int timeout)
		{
			if (!Monitor.TryEnter(_v, timeout))
				return false;
			try
			{
				action(_v);
			}
			finally
			{
				Monitor.Exit(_v);
			}

			return true;
		}


		public V take<V>(Func<T, V> f)
		{
			lock (_v)
			{
				return f(_v);
			}
		}

		#endregion
	}

	public static class SharedExtensions
	{
		public static IShared<T> shared<T>(this T instance)
			where T : class
		{
			return new Shared<T>(instance);
		}
	}
}
