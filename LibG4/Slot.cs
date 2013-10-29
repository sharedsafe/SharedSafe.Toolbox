using System.Collections.Generic;

namespace LibG4
{
	public struct Slot<PropertyT>
	{
		public PropertyT Value;
		public List<Transaction> Readers;
	}
}