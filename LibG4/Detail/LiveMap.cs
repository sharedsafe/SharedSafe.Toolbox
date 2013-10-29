using System;
using System.Diagnostics;
using Toolbox;

namespace LibG4.Detail
{
	sealed class LiveMap<FromT, ToT> : IDisposable, ICollectionChange<FromT>
	{
		readonly Collection<FromT> _source;
#if DEBUG
		Collection<ToT> _target;
#else
		readonly Collection<ToT> _target;
#endif
		readonly Func<FromT, ToT> _constructor;
		readonly Action<ToT> _destructor;

		public LiveMap(Collection<FromT> source, Func<FromT, ToT> constructor)
			: this(source, constructor, null)
		{
		}

		public void Dispose()
		{
			// spot double disposes in DEBUG builds!
			Debug.Assert(_target != null);

			// note: calls back to collection changed and so destructs all objects
			_target.Clear();
			_source.OnCollectionChanged -= onChanged;

#if DEBUG
			_target = null;
#endif
		}

		public LiveMap(Collection<FromT> source, Func<FromT, ToT> constructor, Action<ToT> destructor)
		{
			Debug.Assert(source != null && constructor != null);

			_source = source;
			_target = new Collection<ToT>(source.Count.unsigned());
			_constructor = constructor;
			_destructor = destructor;

			_source.OnCollectionChanged += onChanged;

			foreach (var from in _source)
				_target.Add(constructor(from));
		}

		void onChanged(Action<ICollectionChange<FromT>> action)
		{
			action(this);
		}

		#region ICollectionChange<FromT> Members

		public void insert(uint index, FromT value)
		{
			_target.Insert(index.signed(), _constructor(value));
		}

		public void replace(uint index, FromT value)
		{
			if (_destructor != null)
				_destructor(_target[index]);

			_target[index] = _constructor(value);
		}

		public void remove(uint index)
		{
			if (_destructor != null)
				_destructor(_target[index]);

			_target.RemoveAt(index.signed());
		}

		public void clear()
		{
			if (_destructor != null)
				foreach (var t in _target)
					_destructor(t);

			_target.Clear();
		}

		#endregion
	}

}

