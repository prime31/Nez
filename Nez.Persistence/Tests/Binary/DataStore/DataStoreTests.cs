using System.IO;
using NUnit.Framework;
using Ass = NUnit.Framework.Assert;

namespace Nez.Persistence.Binary.Tests
{
    public class DataStoreTests
    {
        FileDataStore _store;
        string _tmpFolder;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {}

        [SetUp]
        public void Setup()
        {
            _tmpFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
            Directory.CreateDirectory(_tmpFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (_store != null)
            {
                _store.Clear();
                _store = null;
            }

            if (Directory.Exists(_tmpFolder))
                Directory.Delete(_tmpFolder, true);
        }

        #region KeyValue Tests

        [Test]
        public void KeyValueDataStore_StartsEmpty()
        {
            var kvStore = new KeyValueDataStore();
            Ass.That(kvStore.ContainsBoolKey("key"), Is.False);
        }

        [Test]
        public void KeyValueDataStore_StoresData()
        {
            var kvStore = new KeyValueDataStore();
            kvStore.Set("key", true);
            Ass.That(kvStore.ContainsBoolKey("key"), Is.True);
            Ass.That(kvStore.GetBool("key"), Is.True);
        }

        [Test]
        public void KeyValueDataStore_AllowsDeletion()
        {
            var kvStore = new KeyValueDataStore();
            kvStore.Set("key", true);
            kvStore.DeleteBoolKey("key");
            Ass.That(kvStore.ContainsBoolKey("key"), Is.False);
        }

        [Test]
        public void KeyValueDataStore_AllowsDeletionOfAllData()
        {
            var kvStore = new KeyValueDataStore();
            kvStore.Set("key", true);
            kvStore.DeleteAll();
            Ass.That(kvStore.ContainsBoolKey("key"), Is.False);
        }

        [Test]
        public void KeyValueDataStore_StartsNotDirty()
        {
            var kvStore = new KeyValueDataStore();
            Ass.IsFalse(kvStore.IsDirty);
        }

        [Test]
        public void KeyValueDataStore_IsNotDirtyDeletingUnusedKey()
        {
            var kvStore = new KeyValueDataStore();
            kvStore.DeleteBoolKey("doesnt-exist");
            Ass.IsFalse(kvStore.IsDirty);
        }

        [Test]
        public void KeyValueDataStore_IsDirtyAfterSettingValue()
        {
            var kvStore = new KeyValueDataStore();
            kvStore.Set("key", true);
            Ass.IsTrue(kvStore.IsDirty);
        }

        [Test]
        public void KeyValueDataStore_SavesAndLoads([Values]FileDataStore.FileFormat format)
        {
            _store = new FileDataStore(_tmpFolder, format);
            var kvStore = new KeyValueDataStore();

            kvStore.Set("bool", TestValues.aBool);
            kvStore.Set("string", TestValues.aString);
            kvStore.Set("float", TestValues.aFloat);
            kvStore.Set("int", TestValues.aInt);

            // save, clear and reload from file
            _store.Save("kv-dump", kvStore);
            kvStore.DeleteAll();
            _store.Load("kv-dump", kvStore);

            File.Copy(Path.Combine(_tmpFolder, "kv-dump"), "/Users/mikedesaro/Desktop/fucker.txt");

            Ass.AreEqual(kvStore.GetBool("bool"), TestValues.aBool);
            Ass.AreEqual(kvStore.GetString("string"), TestValues.aString);
            Ass.AreEqual(kvStore.GetFloat("float"), TestValues.aFloat);
            Ass.AreEqual(kvStore.GetInt("int"), TestValues.aInt);
        }

        #endregion

    }
}