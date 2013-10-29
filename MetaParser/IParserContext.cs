using System;

namespace Toolbox.MetaParser
{
	public interface IParserContext
	{
		int? match(string str);
		int? match(Type type);

		/// The result of the most recent type match
		object Result { get; }
		/// The current instance of the type that is currently being parsed.
		object Instance { get; }
	}

	public struct MatchedType
	{
		public int Consumed;
		public object Instance;
	};
}
