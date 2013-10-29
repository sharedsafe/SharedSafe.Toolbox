
namespace Toolbox.Sync.Detail
{
	/**
		A scope is a non-mutable node in a back-linked name chain.

		It is used in iterating hierarchies to always be able to resolve the concrete
		path we are at.

		An iteration tree always has a common root, which is constant and has only one instance.
	**/

	sealed class Scope : IScope
	{
		readonly IScope _parent;
		readonly string _name;

		Scope(IScope parent, string name)
		{
			_parent = parent;
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public IScope Parent_
		{
			get { return _parent; }
		}

		public IScope enter(string name)
		{
			return new Scope(this, name);
		}

		public static readonly Scope Root = new Scope(null, string.Empty);

		public override string ToString()
		{
			if (_parent != null)
			{
				var ps = _parent.ToString();
				return ps.Length != 0 ? ps + "/" + _name : _name;
			}
			return _name;
		}
	}
}
