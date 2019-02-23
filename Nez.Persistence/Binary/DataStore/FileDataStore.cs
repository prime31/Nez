using System.IO;

namespace Nez.Persistence.Binary
{
    public class FileDataStore
    {
        public enum FileFormat
        {
            Binary,
            Text
        }

        FileFormat _fileFormat;
        string _persistentDataPath;

        ReuseableBinaryWriter _writer;
        ReuseableBinaryReader _reader;

        public FileDataStore(string persistentDataPath, FileFormat fileFormat = FileFormat.Binary)
        {
            // var gamePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Watercolor Games", "Peacenet");
            // ensure directory exists
            Directory.CreateDirectory(persistentDataPath);
            _persistentDataPath = persistentDataPath;
            _fileFormat = fileFormat;
            if (_fileFormat == FileFormat.Binary)
            {
                // if we are binary, cache our reader/writer since they are reuseable
                _writer = new ReuseableBinaryWriter();
                _reader = new ReuseableBinaryReader();
            }
        }

        #region IDataStore

        public void Save(string filename, IPersistable persistable)
        {
            var tmpFile = GetTmpFile(filename);
            using (var writer = GetDataWriter(tmpFile))
                writer.Write(persistable);

            File.Copy(tmpFile, Path.Combine(_persistentDataPath, filename), true);
            File.Delete(tmpFile);
        }

        public void Load(string filename, IPersistable persistable, string legacyKey = null)
        {
            var file = Path.Combine(_persistentDataPath, filename);
            if (File.Exists(file))
            {
                using (var reader = GetDataReader(file))
                    reader.ReadPersistableInto(persistable);
            }
        }

        public void Clear()
        {
            var di = new DirectoryInfo(_persistentDataPath);
            foreach (var file in di.GetFiles())
                file.Delete();
        }

        #endregion

        IPersistableWriter GetDataWriter(string file)
        {
            if (_fileFormat == FileFormat.Binary)
            {
                _writer.SetStream(file);
                return _writer;
            }
            return new TextPersistableWriter(file);
        }

        IPersistableReader GetDataReader(string file)
        {
            if (_fileFormat == FileFormat.Binary)
            {
                _reader.SetStream(file);
                return _reader;
            }
            return new TextPersistableReader(file);
        }

        string GetTmpFile(string filename)
        {
            return Path.Combine(_persistentDataPath, filename + ".tmp");
        }
    }
}