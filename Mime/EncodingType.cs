using Toolbox.Meta;

namespace Toolbox.Mime
{
	[CaseInsensitive, PresentInLowerCase]
	public enum EncodingType
	{
		[PresentAs("7bit")] SevenBit,
		[PresentAs("8bit")] EightBit,
		Binary,
		[PresentAs("quoted-printable")] QuotedPrintable,
		Base64
	};
}
