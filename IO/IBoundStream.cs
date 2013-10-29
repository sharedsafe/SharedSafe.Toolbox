namespace Toolbox.IO
{
	public interface IBoundStream
	{
		ulong Length { get; }
		ulong BytesLeft { get; }
	}
}
