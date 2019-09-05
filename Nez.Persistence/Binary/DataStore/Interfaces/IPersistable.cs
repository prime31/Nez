namespace Nez.Persistence.Binary
{
	public interface IPersistable
	{
		void Recover(IPersistableReader reader);
		void Persist(IPersistableWriter writer);
	}
}