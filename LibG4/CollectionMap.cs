// todo: is this used anymore?

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox;

namespace LibG4
{
	public static class CollectionExtensions
	{
		public static void map<FromT, ToT>(this Collection<FromT> source, Collection<ToT> target, Func<FromT, ToT> func)
		{
			Debug.Assert(target != null && func != null);

			var mapper = new CollectionMap<FromT, ToT>(source, target, func);
			G4.attach(mapper);
		}

		sealed class CollectionMap<FromT, ToT> : IDisposable, ICollectionChange<FromT>
		{
			readonly Collection<FromT> _source;
			readonly Collection<ToT> _target;
			readonly Func<FromT, ToT> _func;
			readonly List<IDisposable> _lives = new List<IDisposable>();

			public CollectionMap(Collection<FromT> source, Collection<ToT> target, Func<FromT, ToT> func)
			{
				_source = source;
				_source.OnCollectionChanged += doCollectionChanged;
				_target = target;
				_func = func;

				// initial setup (without change notification, so we directly access the buffer)
				// tbd: enumeration of a collection should not do a full change notification, or should it?

				FromT[] buf = source.Buffer;

				for (uint i = 0; i != source.Count; ++i)
					insert(i, buf[i]);
			}

			public void Dispose()
			{
				_source.OnCollectionChanged -= doCollectionChanged;
				// notw: if the mapper is disposed, the target list is cleared!
				clear();
			}

			void doCollectionChanged(Action<ICollectionChange<FromT>> action)
			{
				action(this);
			}

			#region ICollectionChange<FromT> Members

			public void insert(uint index, FromT value)
			{
				if (index != _target.Count)
					throw new NotImplementedException();

				int offset = _target.Count;

				_target.Add(default(ToT));

				// don't know if we have to track lives here, if we manage a slot for each
				// entry in the colleciton, lives should be removed from the back-references then!

				_lives.Add(G4.live(() =>
					{
						_target[offset] = _func(_source[offset]);
					}));
			}

			public void replace(uint index, FromT value)
			{
				_lives[index.signed()].Dispose();
				_lives[index.signed()] = G4.live(() =>
					{
						_target[index] = _func(_source[index]);
					});
			}

			public void remove(uint index)
			{
				if (index != _target.Count-1)
					throw new NotImplementedException();

				_lives[(int)index].Dispose();
				_lives.RemoveAt((int)index);

				_target.RemoveAt((int)index);
			}

			public void clear()
			{
				_lives.ForEach(d => d.Dispose());
				_lives.Clear();
				_target.Clear();
			}

			#endregion
		}
	}
}
