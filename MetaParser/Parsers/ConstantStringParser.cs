using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox.MetaParser
{
	class ConstantStringParser : IParser
	{
		readonly string _str;

		public ConstantStringParser(string str)
		{
			_str = str;
		}

		#region IParser Members

		int? IParser.parse(IParserContext context)
		{
			return context.match(_str);
		}

		#endregion
	}
}
