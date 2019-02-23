namespace Nez.Persistence.Binary
{
	public interface IPersistableReader : System.IDisposable
	{
		string ReadString();
		uint ReadUInt();
		int ReadInt();
		float ReadFloat();
		double ReadDouble();
		bool ReadBool();
	}
}