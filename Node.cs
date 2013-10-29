#if false

using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Toolbox
{
	/**
		An acyclic directed graph node. With a proper representation of 
		up and down stream nodes.

		Note: this class is not abstract, so that we can insert additional non-typed nodes at any time in the graph.
	**/

	public class Node : IDisposable
	{
		List<Node> _ups;
		List<Node> _downs;
	
		#region IDisposable Members

		public void Dispose()
		{
			// remove us from all ups and downs.


		}

		#endregion

		/**
			Add a downstream node.
		**/

		public void add(Node node)
		{
			if (node._ups == null)
				node._ups = new List<Node>();

			if (_downs == null)
				_downs = new List<Node>();

			node._ups.Add(this);
			try
			{
				if (_downs != null)
					_downs.Add(node);
			}
			catch (Exception)
			{
				node._ups.Remove(this);
				throw;
			}
		}

		/**
			Remove a downstream node.
		**/
	
		public void remove(Node node)
		{
			Debug.Assert(node._ups != null);
			Debug.Assert(_downs != null);

			bool removed = node._ups.Remove(this);
			Debug.Assert(removed);
			try
			{
				removed = _downs.Remove(node);
				Debug.Assert(removed);
			}
			catch (Exception)
			{
				node._ups.Add(this);
				throw;
			}
		}

		/**
			Call an action for each upstream node.
		**/

		public void forEachUp<TypeT>(Action<TypeT> a)
			where TypeT : class
		{
			if (_ups == null)
				return;

			_ups.ForEach(n => 
				{
					var b = n as TypeT;
					if (b != null)
						a(b);
				});
		}


		/**
			Call an action for each downstream node.
		**/

		public void forEachDown<TypeT>(Action<TypeT> a)
			where TypeT : class
		{
			if (_downs == null)
				return;

			_ups.ForEach(n =>
			{
				var b = n as TypeT;
				if (b != null)
					a(b);
			});
		}

		// pull changes from dependent nodes
		// todo: this is an extension method!

		protected void pullChanges()
		{
			forEachDown<ChangeNode>(node => node.pushChangesUp());
		}
	}

	abstract class ChangeNode : Node
	{
		abstract public void pushChangesUp();
	}

	abstract class ChangeNode<IType> : ChangeNode
		where IType : class
	{
		readonly Queue<Action<IType>> _changes = new Queue<Action<IType>>();

		public override void pushChangesUp()
		{
			forEachUp<IType>(
				i =>
					{
						foreach (var a in _changes)
							a(i);

					}
				);

			_changes.Clear();
		}

		protected void record(Action<IType> change)
		{
			_changes.Enqueue(change);
		}
	}
}

#endif
