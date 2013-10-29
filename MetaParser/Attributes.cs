#if false

using System;
using System.Linq;

namespace Toolbox.MetaParser
{
	#region Field Attributes

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class OneOfAttribute : ParserAttribute
	{
		readonly Type[] _types;

		public OneOfAttribute(params Type[] types)
		{
			_types = types;
		}

		internal IParser makeParser()
		{
			return new OneOfParser((from t in _types select ParserGenerator.makeParser(t)).ToArray());
		}
	};

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class OptionalAttribute : ParserAttribute
	{

	}

	#endregion

	#region Class And Field Attributes

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
	public sealed class PrefixAttribute : ParserAttribute
	{
		readonly object[] _objects;

		public PrefixAttribute(params object[] objects)
		{
			_objects = objects;
		}

		public IParser makeParser()
		{
			return ParserGenerator.createSequenceParser(_objects);
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
	public sealed class PostfixAttribute : ParserAttribute
	{
		readonly object[] _objects;

		public PostfixAttribute(params object[] objects)
		{
			_objects = objects;
		}

		public IParser createParser()
		{
			return ParserGenerator.createSequenceParser(_objects);
		}
	}

	#endregion

	public abstract class ParserAttribute : Attribute
	{
		// internal abstract Parser createParser();
	}
}

#endif
