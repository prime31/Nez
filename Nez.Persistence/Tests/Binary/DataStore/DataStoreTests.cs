using System.IO;
using NUnit.Framework;

namespace Nez.Persistence.Binary.Tests
{
	public class DataStoreTests
	{
		FileDataStore _store;
		string _tmpFolder;
		string _filename = "kv-test.bin";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{ }

		[SetUp]
		public void Setup()
		{
			_tmpFolder = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName().Replace( ".", "" ) );
			Directory.CreateDirectory( _tmpFolder );
		}

		[TearDown]
		public void TearDown()
		{
			if( _store != null )
			{
				_store.Clear();
				_store = null;
			}

			if( Directory.Exists( _tmpFolder ) )
				Directory.Delete( _tmpFolder, true );
		}

		#region KeyValue Tests

		[Test]
		public void KeyValueDataStore_StartsEmpty()
		{
			var kvStore = new KeyValueDataStore( _filename );
			Assert.That( kvStore.ContainsBoolKey( "key" ), Is.False );
		}

		[Test]
		public void KeyValueDataStore_StoresData()
		{
			var kvStore = new KeyValueDataStore( _filename );
			kvStore.Set( "key", true );
			Assert.That( kvStore.ContainsBoolKey( "key" ), Is.True );
			Assert.That( kvStore.GetBool( "key" ), Is.True );
		}

		[Test]
		public void KeyValueDataStore_AllowsDeletion()
		{
			var kvStore = new KeyValueDataStore( _filename );
			kvStore.Set( "key", true );
			kvStore.DeleteBoolKey( "key" );
			Assert.That( kvStore.ContainsBoolKey( "key" ), Is.False );
		}

		[Test]
		public void KeyValueDataStore_AllowsDeletionOfAllData()
		{
			var kvStore = new KeyValueDataStore( _filename );
			kvStore.Set( "key", true );
			kvStore.DeleteAll();
			Assert.That( kvStore.ContainsBoolKey( "key" ), Is.False );
		}

		[Test]
		public void KeyValueDataStore_StartsNotDirty()
		{
			var kvStore = new KeyValueDataStore( _filename );
			Assert.IsFalse( kvStore.IsDirty );
		}

		[Test]
		public void KeyValueDataStore_IsNotDirtyDeletingUnusedKey()
		{
			var kvStore = new KeyValueDataStore( _filename );
			kvStore.DeleteBoolKey( "doesnt-exist" );
			Assert.IsFalse( kvStore.IsDirty );
		}

		[Test]
		public void KeyValueDataStore_IsDirtyAfterSettingValue()
		{
			var kvStore = new KeyValueDataStore( _filename );
			kvStore.Set( "key", true );
			Assert.IsTrue( kvStore.IsDirty );
		}

		[Test]
		public void KeyValueDataStore_SavesAndLoads( [Values]FileDataStore.FileFormat format )
		{
			_store = new FileDataStore( _tmpFolder, format );
			var kvStore = new KeyValueDataStore( _filename );

			kvStore.Set( "bool", TestValues.aBool );
			kvStore.Set( "string", TestValues.aString );
			kvStore.Set( "float", TestValues.aFloat );
			kvStore.Set( "int", TestValues.aInt );

			// save, clear and reload from file
			_store.Save( "kv-dump", kvStore );
			kvStore.DeleteAll();
			_store.Load( "kv-dump", kvStore );

			Assert.AreEqual( kvStore.GetBool( "bool" ), TestValues.aBool );
			Assert.AreEqual( kvStore.GetString( "string" ), TestValues.aString );
			Assert.AreEqual( kvStore.GetFloat( "float" ), TestValues.aFloat );
			Assert.AreEqual( kvStore.GetInt( "int" ), TestValues.aInt );
		}

		#endregion

	}
}