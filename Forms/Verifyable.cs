
namespace Toolbox.Forms
{
	public interface IVerifiable
	{
		bool verify();
		string Hint_ { get; }
	}
}
