/**
	Represents an instantiated integrator with all its live behaviors.
**/

using System;
using System.Collections.Generic;

namespace LibG4.Detail
{
	sealed class Integration : IDisposable
	{
		readonly IntegrationSpace _integrationSpace;

		/// The current destrutor.
		Action _destructor;

		/// Dictionary of type key maintenance actions.
		Dictionary<Type, Action> _contained;

		public Integration(IntegrationSpace integrationSpace)
		{
			_integrationSpace = integrationSpace;
		}

		public void Dispose()
		{
			if (_contained != null)
			{
				foreach (var destructor in _contained.Values)
					destructor();
				_contained = null;
			}

			if (_destructor != null)
				_destructor();
		}

		/**
			Add a new maintenance function to this runtime integrator.

			A maintenance function has the livetime scope of the runtime integrator and is rerun 
			whenever accessed model properties are changed.
		**/

		public void maintain(Action func)
		{
			destructor(G4.live(func).Dispose);
		}

		public void maintain<KeyT>(Action func)
		{
			if (_contained == null)
				_contained = new Dictionary<Type, Action>();

			var keyT = typeof(KeyT);
			Action fBefore;
			bool exists = false;
			if (_contained.TryGetValue(keyT, out fBefore))
			{
				fBefore();
				exists = true;
			}

			if (func != null)
				_contained[keyT] = G4.live(func).Dispose;
			else if (exists)
				if (!_contained.Remove(keyT))
					throw new Exception("internal error, removing of maintenance function failed");
		}

		public void maintain<KeyT>()
		{
			maintain<KeyT>(null);
		}

		/**
			Add a new behavior to this runtime integrator.

			This runs the behavior and adds its return value to the destructor chain.

		**/

		public void behavior(Func<Action> behavior)
		{
			destructor(behavior());
		}

		public void destructor(Action destructor)
		{
			if (destructor == null)
				return;

			if (_destructor == null)
			{
				_destructor = destructor;
				return;
			}

			// destruction of behaviors is inversed!
			// todo: analyze costs of chaining destructors and may optimize
			// by organizing them in an Action[] array.

			var previous = _destructor;
			_destructor = () =>
				{
					destructor();
					previous();
				};
		}

		public void integrate<ContextT, MT>(Collection<MT> collection, ContextT context)
		{
			var lm = new LiveMap<MT, Integration>(
				collection,
				m => _integrationSpace.integrate(m, context),
				d => d.Dispose());

			destructor(lm.Dispose);
		}
	}
}
