/**
	Specifies that a value can be interpolated.
**/

namespace Toolbox.Geometry
{
	public interface IInterpolatable<Value>
	{
		Value interpolate(double coeff, Value target);
	}
}
