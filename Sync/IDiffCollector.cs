namespace Toolbox.Sync
{
	public enum DiffKind
	{
		/// Path is new.
		New,
		/// Path has been deleted
		Deleted,
		/// The type is the same, but contents / attributes have been changed.
		Changed
	};

	public interface IDiffCollector
	{
		void compare(IScope scope, IItem source, IItem target);

		void collect(IScope scope, string name, DiffKind kind);

		IDirtyPath createDirtyPath();
		SyncOptions Options { get; }
	}
}
