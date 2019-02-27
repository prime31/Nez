using Nez.Persistence;
using NUnit.Framework;
using System;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class StructTypeTests
	{
		public static bool LoadCallbackFired;

		struct TestStruct
		{
			public int x;
			public int y;

			[JsonExclude]
			public int z;

			[AfterDecodeAttribute]
			public void OnLoad()
			{
				LoadCallbackFired = true;
			}
		}


		[Test]
		public void DumpStruct()
		{
			var testStruct = new TestStruct { x = 5, y = 7, z = 0 };

			Assert.AreEqual( "{\"x\":5,\"y\":7}", Json.ToJson( testStruct ) );
		}


		[Test]
		public void LoadStruct()
		{
			var testStruct = Json.FromJson<TestStruct>( "{\"x\":5,\"y\":7,\"z\":3}" );

			Assert.AreEqual( 5, testStruct.x );
			Assert.AreEqual( 7, testStruct.y );
			Assert.AreEqual( 0, testStruct.z ); // should not get assigned

			Assert.IsTrue( LoadCallbackFired );
		}
	}
}