using System.Collections.Generic;
using System.IO;

namespace Nez.Persistence.Binary
{
    public class BinaryPersistableReader : System.IO.BinaryReader, IPersistableReader
    {
        public BinaryPersistableReader(string filename) : base(File.OpenRead(filename))
        {}

        public BinaryPersistableReader(Stream input) : base(input)
        {}

        public uint ReadUInt() => ReadUInt32();

        public int ReadInt() => ReadInt32();

        public float ReadFloat() => ReadSingle();

        public bool ReadBool() => ReadBoolean();

    }
}