namespace RootSE.Provider
{
	public sealed class Limit
	{
		readonly uint _number;

		Limit(uint number)
		{
			_number = number;
		}

		public string SQL { get { return ValueEncoder.encode(_number, null); } }

		public static readonly Limit One = new Limit(1);
	}
}
