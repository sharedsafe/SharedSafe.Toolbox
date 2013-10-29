#if false

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbox.Meta;

namespace Toolbox.MetaParser
{
	static class ParserGenerator
	{
		public static IParser makeParser(Type type)
		{
			IParser parser;
			if (TypeParsers.TryGetValue(type, out parser))
				return parser;

			parser = createParser(type);
			TypeParsers[type] = parser;
			return parser;
		}

		static IParser createParser(Type type)
		{
			var elementParsers = new List<IParser>();

			var prefix = type.queryAttribute<PrefixAttribute>();
			if (prefix != null)
				elementParsers.Add(prefix.makeParser());

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (var field in fields)
			{
				var fieldParser = generateFieldParser(field);
				if (fieldParser != null)
					elementParsers.Add(fieldParser);
			}

			var postfix = type.queryAttribute<PostfixAttribute>();
			if (postfix != null)
				elementParsers.Add(postfix.createParser());

			return makeSequence(elementParsers.ToArray());
		}

		static IParser generateFieldParser(FieldInfo field)
		{
			var optional = field.queryAttribute<OptionalAttribute>();
			
			
			var prefix = field.queryAttribute<PrefixAttribute>();
			var postfix = field.queryAttribute<PostfixAttribute>();
			
			/// OneOf defines the content parser, if not set, it is the
			/// field type's parser.
			var content = field.queryAttribute<OneOfAttribute>();

			var contentParser = content != null ? content.makeParser() : makeParser(field.FieldType);

			var contentParsers = new List<IParser>();
			if (prefix != null)
				contentParsers.Add(prefix.makeParser());
			contentParsers.Add(contentParser);
			if (postfix != null)
				contentParsers.Add(postfix.createParser());
			
			var result = makeSequence(contentParsers.ToArray());

			if (optional != null)
				result = new OptionalParser(result);

			return result;
		}

		/**
			Don't want an unnecessary squence to be generated, if we only have one parser here!
		**/

		static IParser makeSequence(params IParser[] ps)
		{
			var parsers = from p in ps where p != null select p;

			Debug.Assert(parsers.Length != 0);
			if (parsers.Length == 1)
				return parsers[0];

			return new SequenceParser(parsers);
		}

		/**
			Create a sequence parser for a number of known objects.
			Right now we support constant strings and Types.
		**/

		internal static IParser createSequenceParser(params object[] objects)
		{
			return makeSequence(from o in objects select o => generateParser(o));
		}

		static IParser generateParser(object obj)
		{
			switch (obj.GetType())
			{
				case typeof(string):
					break;

				case typeof(Type):
					break;
			}
		}

		static Dictionary<Type, IParser> TypeParsers = new Dictionary<Type, IParser>();
	}
}

#endif
