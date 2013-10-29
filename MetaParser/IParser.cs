namespace Toolbox.MetaParser
{
	public interface IParser
	{
		// Returns the number of parsed input items or null if parsing failed.

		int? parse(IParserContext context);
	}
}
