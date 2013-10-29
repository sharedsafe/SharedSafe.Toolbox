#if false

namespace Toolbox.MetaParser
{
	sealed class OneOfParser : IParser
	{
		readonly IParser _parsers;

		public OneOfParser(IParser[] parsers)
		{
			_parsers = parsers;
		}

		#region IParser Members

		public int? parse(IParserContext context)
		{




		}

		#endregion
	}
}

#endif
