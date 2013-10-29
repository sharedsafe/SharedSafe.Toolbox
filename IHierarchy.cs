using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox
{
	public interface IHierarchy<TypeT>
		where TypeT : class
	{
		IEnumerable<TypeT> Nested { get; }
	}

	public static class HierarchyExtensions
	{
		public static IEnumerable<TypeT> traversePreOrdered<TypeT>(this TypeT root)
			where TypeT: class, IHierarchy<TypeT>
		{
			yield return root;

			foreach (var nested in root.Nested)
				foreach (var a in nested.traversePreOrdered())
					yield return a;
		}

		public static IEnumerable<TypeT> traversePostOrdered<TypeT>(this TypeT root)
			where TypeT: class, IHierarchy<TypeT>
		{
			foreach (var nested in root.Nested)
				foreach (var a in nested.traversePostOrdered())
					yield return a;

			yield return root;
		}

		#region Traversal with path information

		/**
			Optimization: If the IEnumerable<Type> is just valid for the time of the call to action, we
			could use Lists and avoid creating all these Chain<> instances.
		**/

		public static void traversePreOrdered<TypeT>(this TypeT root, Action<IEnumerable<TypeT>, TypeT> action)
			where TypeT: class, IHierarchy<TypeT>
		{
			var tree = new TreeTraversal<TypeT>(action);
			tree.traversePreOrdered(root, null);
		}

		public static void traversePostOrdered<TypeT>(this TypeT root, Action<IEnumerable<TypeT>, TypeT> action)
			where TypeT : class, IHierarchy<TypeT>
		{
			var tree = new TreeTraversal<TypeT>(action);
			tree.traversePostOrdered(root, null);
		}

		sealed class TreeTraversal<TypeT>
			where TypeT : class, IHierarchy<TypeT>
		{
			readonly Action<IEnumerable<TypeT>, TypeT> _action;

			public TreeTraversal(Action<IEnumerable<TypeT>, TypeT> action)
			{
				_action = action;
			}


			public void traversePreOrdered(TypeT root, Chain<TypeT> chain_)
			{
				_action(chain_ ?? Enumerable.Empty<TypeT>(), root);

				Chain<TypeT> subChain = null;

				foreach (var nested in root.Nested)
				{
					if (subChain == null)
						subChain = new Chain<TypeT>(chain_, root);

					traversePreOrdered(nested, subChain);
				}
			}

			public void traversePostOrdered(TypeT root, Chain<TypeT> chain_)
			{
				Chain<TypeT> subChain = null;

				foreach (var nested in root.Nested)
				{
					if (subChain == null)
						subChain = new Chain<TypeT>(chain_, root);

					traversePostOrdered(nested, subChain);
				}

				_action(chain_ ?? Enumerable.Empty<TypeT>(), root);
			}
		}

		#endregion
	}
}
