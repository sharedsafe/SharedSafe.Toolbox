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
    sealed class NullExpression : ExpressionBase
    {
        public override object Evaluate(SerializationContext context)
        {
            return null;
        }

        public override object GetReference(SerializationContext context)
        {
            throw new Exception("Cannot reference null");
        }

        public override ExpressionBase ResolveReference(ReferenceIdentifier refID)
        {
            throw new Exception("Cannot reference null");
        }
    }
}
