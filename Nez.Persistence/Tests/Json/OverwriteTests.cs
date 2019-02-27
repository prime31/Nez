using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class OverwriteTests
	{
		class TestClass
		{
			public int x;
			public int y;

			[JsonExclude]
			public int z;

			public List<int> list;

			public int p1 { get; set; }

			public int p2 { get; private set; }
			public int p3 { get; }


			public TestClass()
			{
				p1 = 1;
				p2 = 2;
				p3 = 3;
			}
		}


		[Test]
		public void OverwriteField()
		{
			var json = "{\"x\":5,\"y\":7,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}";

			var testClass = Json.FromJson<TestClass>( json );
			Assert.AreEqual( 5, testClass.x );

			Json.FromJsonOverwrite( "{\"x\":10}", testClass );
			Assert.AreEqual( 10, testClass.x );
			Assert.AreEqual( 7, testClass.y );

			Assert.AreEqual( 3, testClass.list.Count );
			Assert.AreEqual( 3, testClass.list[0] );
			Assert.AreEqual( 1, testClass.list[1] );
			Assert.AreEqual( 4, testClass.list[2] );

			Assert.AreEqual( 1, testClass.p1 );
			Assert.AreEqual( 2, testClass.p2 );
			Assert.AreEqual( 3, testClass.p3 );
		}


		[Test]
		public void OverwriteArray()
		{
			var json = "{\"x\":5,\"y\":7,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}";

			var testClass = Json.FromJson<TestClass>( json );
			Assert.AreEqual( 3, testClass.list.Count );

			Json.FromJsonOverwrite( "{\"list\":[5,6,7,8]}", testClass );
			Assert.AreEqual( 5, testClass.x );
			Assert.AreEqual( 7, testClass.y );

			Assert.AreEqual( 4, testClass.list.Count );
			Assert.AreEqual( 5, testClass.list[0] );
			Assert.AreEqual( 6, testClass.list[1] );
			Assert.AreEqual( 7, testClass.list[2] );
			Assert.AreEqual( 8, testClass.list[3] );

			Assert.AreEqual( 1, testClass.p1 );
			Assert.AreEqual( 2, testClass.p2 );
			Assert.AreEqual( 3, testClass.p3 );
		}

	}
}