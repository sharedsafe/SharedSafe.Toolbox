/*
 * Copyright (c) 2007, Ted Elliott
 * Code licensed under the New BSD License:
 * http://code.google.com/p/jsonexserializer/wiki/License
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonExSerializer.Expression
{
    /// <summary>
    /// Expression to represent a javascript List/Array
    /// </summary>
    public sealed class ListExpression : ComplexExpressionBase {
        private IList<ExpressionBase> _items;

        public ListExpression()
        {
            _items = new List<ExpressionBase>();
        }

        public IList<ExpressionBase> Items
        {
            get { return this._items; }
            set { this._items = value; }
        }

        public void Add(ExpressionBase item)
        {
            _items.Add(item);
            item.Parent = this;
        }

        /// <summary>
        /// Resolve a reference to an item within the collection
        /// </summary>
        /// <param name="refID">the reference to resolve</param>
        /// <returns>the referenced expression</returns>
        protected override ExpressionBase ResolveChildReference(ReferenceIdentifier refID)
        {
            int index = refID.CurrentIndex;
            if (index < 0 || index >= _items.Count)
            {
                throw new ArgumentOutOfRangeException("Reference to collection item out of range: " + refID);
            }
            ExpressionBase expr = _items[index];
            return expr.ResolveReference(refID);
        }

    }
}
