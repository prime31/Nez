using System.IO;


namespace Nez.Persistence.Binary
{
	public class TextPersistableWriter : StreamWriter, IPersistableWriter
	{
		public TextPersistableWriter(string filename) : base(File.OpenWrite(filename))
		{
		}

		public TextPersistableWriter(Stream stream) : base(stream)
		{
		}

		public new void Write(string value)
		{
			WriteLine(value.Length);
			WriteLine(value);
		}

		public new void Write(uint value) => WriteLine(value);

		public new void Write(int value) => WriteLine(value);

		public new void Write(float value) => WriteLine(value);

		public new void Write(double value) => WriteLine(value);

		public new void Write(bool value) => WriteLine(value);

		public void Write(IPersistable value) => value.Persist(this);
	}
}