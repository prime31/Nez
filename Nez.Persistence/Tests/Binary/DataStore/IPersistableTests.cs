using System.IO;
using NUnit.Framework;

namespace Nez.Persistence.Binary.Tests
{
	public class IPersistableTests
	{
		FileDataStore _store;
		string _tmpFolder;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{ }

		[SetUp]
		public void Setup()
		{
			_tmpFolder = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName().Replace( ".", "" ) );
			Directory.CreateDirectory( _tmpFolder );
			_store = new FileDataStore( _tmpFolder );
		}

		[TearDown]
		public void TearDown()
		{
			_store.Clear();
			if( Directory.Exists( _tmpFolder ) )
				Directory.Delete( _tmpFolder, true );
		}

		[Test]
		public void IPersistable_DiffersFromDefault()
		{
			var obj = new PersistableExample();
			obj.anInt = 666;
			obj.nullableBool = false;
			obj.strings.Add( "one" );
			obj.strings.Add( "two" );
			obj.strings.Add( "three" );

			_store.Save( "persist-me.bin", obj );

			var newObj = new PersistableExample();

			Assert.AreNotEqual( obj, newObj );
		}


		[Test]
		public void IPersistable_Persists()
		{
			var obj = new PersistableExample();
			obj.anInt = 666;
			obj.nullableBool = false;
			obj.strings.Add( "one" );
			obj.strings.Add( "two" );
			obj.strings.Add( "three" );

			_store.Save( "persist-me.bin", obj );

			var newObj = new PersistableExample();
			_store.Load( "persist-me.bin", newObj );

			Assert.AreEqual( obj, newObj );
		}

	}
}