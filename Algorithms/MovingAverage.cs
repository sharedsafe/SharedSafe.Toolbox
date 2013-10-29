namespace Toolbox.Algorithms
{
	public class ExponentialMovingAverage
	{
		readonly double _smoothingFactor;
		double _latest;

		public ExponentialMovingAverage(uint periods)
			: this (2.0 / (periods +1))
		{
		}

		public ExponentialMovingAverage(double smoothingFactor)
		{
			_smoothingFactor = smoothingFactor;
		}

		public double computeNext(double current)
		{
			_latest = _latest + _smoothingFactor*(current - _latest);
			return _latest;
		}
	}
}
