/*
 * Copyright (c) 2007, Ted Elliott
 * Code licensed under the New BSD License:
 * http://code.google.com/p/jsonexserializer/wiki/License
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using System.Reflection;
using JsonExSerializer.MetaData;

namespace JsonExSerializer.Expression
{
    /// <summary>
    /// Evaluator for Object expressions
    /// </summary>
    sealed class ObjectEvaluator : ComplexEvaluatorBase
    {

        public ObjectEvaluator(ObjectExpression expression)
            : base(expression)
        {
        }

        protected override object Construct()
        {
            // set the default type if none set
            Expression.SetResultTypeIfNotSet(typeof(Hashtable));
			// armin: if we find a __type, we want to change the ResultType to a derived one.
			adjustTypeTo__type();

            if (Expression.ConstructorArguments.Count > 0)
            {
                TypeHandler handler = Context.GetTypeHandler(Expression.ResultType);
                Type[] definedTypes = GetConstructorParameterTypes(handler.ConstructorParameters);

                CtorArgTypeResolver resolver = new CtorArgTypeResolver(Expression, this.Context, definedTypes);
                Type[] resolvedTypes = resolver.ResolveTypes();
                for (int i = 0; i < resolvedTypes.Length; i++)
                {
                    if (resolvedTypes[i] != null)
                        Expression.ConstructorArguments[i].SetResultTypeIfNotSet(resolvedTypes[i]);
                }
            }
            return base.Construct();
        }

		void adjustTypeTo__type()
		{
			var properties = Expression.Properties;

			for(int i = 0; i != properties.Count; ++i)
			{
				if (properties[i].Key != "__type")
					continue;

				changeExpressionResultType(properties[i].ValueExpression);
				properties.RemoveAt(i);
				return;
			}
		}

		void changeExpressionResultType(ExpressionBase value)
		{
			var typeName = value.Evaluate(Context) as string;
			if (typeName == null)
				throw new Exception("__type must be evaluatable as string");
			var t = Context.GetTypeBinding(typeName);
			if (t == null)
				throw new Exception(string.Format("Missing type binding for __type {0}", typeName));
			if (!Expression.ResultType.IsAssignableFrom(t))
				throw new Exception(string.Format("__type {0} is not assignable to expected type {1}", t, Expression.ResultType));
			Expression.ResultType = t;
		}

        private Type[] GetConstructorParameterTypes(IList<AbstractPropertyHandler> ConstructorParameters)
        {
            Type[] types = new Type[ConstructorParameters.Count];
            for (int i = 0; i < ConstructorParameters.Count; i++)
            {
                types[i] = ConstructorParameters[i].PropertyType;
            }
            return types;
        }
        /// <summary>
        /// Populate the list with its values
        /// </summary>
        protected override void UpdateResult()
        {            
            foreach (KeyValueExpression Item in Expression.Properties)
            {
                // evaluate the item and let it assign itself?
                Item.Evaluate(Context, this._result);
            }
        }

        public new ObjectExpression Expression
        {
            get { return (ObjectExpression)_expression; }
            set { _expression = value; }
        }
    }
}
