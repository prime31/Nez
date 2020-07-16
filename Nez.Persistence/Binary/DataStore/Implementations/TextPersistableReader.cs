using System.IO;


namespace Nez.Persistence.Binary
{
	public class TextPersistableReader : StreamReader, IPersistableReader
	{
		public TextPersistableReader(string filename) : base(File.OpenRead(filename))
		{
		}

		public TextPersistableReader(Stream stream) : base(stream)
		{
		}

		public string ReadString()
		{
			var length = ReadInt();
			var buff = new char[length];
			ReadBlock(buff, 0, length);

			// chomp the newline that is after our string
			ReadLine();

			return new string(buff);
		}


		public bool ReadBool() => bool.Parse(ReadLine());

		public double ReadDouble() => double.Parse(ReadLine());

		public float ReadFloat() => float.Parse(ReadLine());


		public int ReadInt() => int.Parse(ReadLine());

		public uint ReadUInt() => uint.Parse(ReadLine());
	}
}