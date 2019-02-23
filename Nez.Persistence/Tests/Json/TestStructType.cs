using Nez.Persistance;
using NUnit.Framework;
using System;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestStructType
	{
		public static bool LoadCallbackFired;

		struct TestStruct
		{
			public int x;
			public int y;

			[NonSerialized]
			public int z;

			[AfterDecodeAttribute]
			public void OnLoad()
			{
				LoadCallbackFired = true;
			}
		}


		[Test]
		public void TestDumpStruct()
		{
			var testStruct = new TestStruct { x = 5, y = 7, z = 0 };

			Assert.AreEqual( "{\"x\":5,\"y\":7}", Json.Encode( testStruct ) );
		}


		[Test]
		public void TestLoadStruct()
		{
			var testStruct = VariantConverter.Make<TestStruct>( Json.Decode( "{\"x\":5,\"y\":7,\"z\":3}" ) );

			Assert.AreEqual( 5, testStruct.x );
			Assert.AreEqual( 7, testStruct.y );
			Assert.AreEqual( 0, testStruct.z ); // should not get assigned

			Assert.IsTrue( LoadCallbackFired );
		}
	}
}