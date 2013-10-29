#if false

using System.Reflection;

namespace Toolbox.MetaParser
{
	class FieldParser : IParser
	{
		readonly FieldInfo _info;

		public FieldParser(FieldInfo info, IParser parser)
		{
			_info = info;
			_parser = parser;
		}
	
		readonly IParser _parser;

		#region IParser Members

		public int? parse(IParserContext context)
		{
			int? r = _parser.parser(context);
			if (r == null)
				return null;

			// apply field value if there is an instance.
			
			var instance = context.Instance;
			if (instance != null)
				_info.SetValue(instance, context.CurrentResult);

			return r;
		}

		#endregion
	}
}

#endif
