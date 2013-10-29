/**
	A IntegrationSpace groups a number of Integrators together.
=**/

using System;
using System.Collections.Generic;
using Toolbox;

namespace LibG4.Detail
{
	sealed class IntegrationSpace
	{
		readonly IList<Integrator> _integrators = new List<Integrator>();
		
		public string Name { get; private set; }

		public IntegrationSpace(string name)
		{
			Name = name;
		}

		public IEnumerable<Integrator> Components
		{
			get
			{
				foreach (var c in _integrators)
					yield return c;
			}
		}

		public void define(Integrator c)
		{
			_integrators.Add(c);
		}

		/**
			Create a runtime integrator based on a already existing model instance.

			Returned is a Integration, which represents the Presentation, 
			Control and their various bindings.

			The returned Integration is disposable, so the bindings and the control
			can be safely uninstalled.

			todo: What about a more global define, that is not dependent on the _integrationSpace we
			are currently in, this would make subsequent instatiations (for example in behaviors)
			easier!
		**/

		public Integration integrate<EnvironmentT>(object model, EnvironmentT environment)
		{
			// todo: make this amortized O(1)
			// also, this should scan for the most derived class.

			Type type = model.GetType();
			Type envType = typeof(EnvironmentT);

			foreach (var c in _integrators)
				if (c.ModelType.IsAssignableFrom(type) && c.EnvironmentType.IsAssignableFrom(envType))
					return c.CreateInstance(this, model, environment);

			throw new Exception(ToString() + ": Model {0} not registered, failed to integrate.".format(type.FullName));
		}

		public override string ToString()
		{
			return Name + " space";
		}
	}
}
