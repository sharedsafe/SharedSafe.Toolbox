namespace LibG4
{
	public interface ICollectionChange<ElementT>
	{
		void insert(uint index, ElementT value);
		void replace(uint index, ElementT value);
		void remove(uint index);
		void clear();
	}
}
