using System;
using System.Collections.Generic;
using Nez.Persistance;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestClassType
	{
		public static bool AfterDecodeCallbackFired;
		public static bool BeforeEncodeCallbackFired;


		class TestClass
		{
			public int x;
			public int y;

			[NonSerialized]
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


			[AfterDecodeAttribute]
			public void AfterDecode()
			{
				AfterDecodeCallbackFired = true;
			}


			[BeforeEncodeAttribute]
			public void BeforeDecode()
			{
				BeforeEncodeCallbackFired = true;
			}
		}


		[Test]
		public void TestDumpClass()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			testClass.list = new List<int> { 3, 1, 4 };

			var json = Json.Encode( testClass );
			Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}", json );

			Assert.IsTrue( BeforeEncodeCallbackFired );
		}


		[Test]
		public void TestDumpClassNoTypeHint()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			testClass.list = new List<int> { 3, 1, 4 };

			var settings = new JsonSettings()
			{
				TypeNameHandling = TypeNameHandling.None
			};
			var json = Json.Encode( testClass, settings );
			Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}", json );
		}


		[Test]
		public void TestDumpClassPrettyPrint()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			testClass.list = new List<int> { 3, 1, 4 };

			var settings = new JsonSettings()
			{
				TypeNameHandling = TypeNameHandling.None,
				PrettyPrint = true
			};
			Assert.AreEqual( @"{
	""x"": 5,
	""y"": 7,
	""list"": [
		3,
		1,
		4
	],
	""p1"": 1,
	""p2"": 2,
	""p3"": 3
}", Json.Encode( testClass, settings ) );
		}


		[Test]
		public void TestDumpClassIncludePublicProperties()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":null,\"p1\":1,\"p2\":2,\"p3\":3}", Json.Encode( testClass ) );
		}


		[Test]
		public void TestLoadClass()
		{
			var testClass = VariantConverter.Make<TestClass>( Json.Decode( "{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}" ) );

			Assert.AreEqual( 5, testClass.x );
			Assert.AreEqual( 7, testClass.y );
			Assert.AreEqual( 0, testClass.z ); // should not get assigned

			Assert.AreEqual( 3, testClass.list.Count );
			Assert.AreEqual( 3, testClass.list[0] );
			Assert.AreEqual( 1, testClass.list[1] );
			Assert.AreEqual( 4, testClass.list[2] );

			Assert.AreEqual( 1, testClass.p1 );
			Assert.AreEqual( 2, testClass.p2 );
			Assert.AreEqual( 3, testClass.p3 );

			Assert.IsTrue( AfterDecodeCallbackFired );
		}


		class InnerClass { }

		class OuterClass
		{
			public InnerClass inner;
		}

		[Test]
		public void TestDumpOuterClassWithNoTypeHintPropagatesToInnerClasses()
		{
			var outerClass = new OuterClass();
			outerClass.inner = new InnerClass();
			Assert.AreEqual( "{\"inner\":{}}", Json.Encode( outerClass ) );
		}

	}
}