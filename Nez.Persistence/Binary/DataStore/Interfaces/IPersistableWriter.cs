namespace Nez.Persistence.Binary
{
	public interface IPersistableWriter : System.IDisposable
	{
		void Write(string value);
		void Write(uint value);
		void Write(int value);
		void Write(float value);
		void Write(double value);
		void Write(bool value);
		void Write(IPersistable value);
	}
}