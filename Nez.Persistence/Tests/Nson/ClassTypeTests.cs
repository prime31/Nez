using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Nez.Persistence.NsonTests
{
	[TestFixture]
	public class ClassTypeTests
	{
		public static bool AfterDecodeCallbackFired;
		public static bool BeforeEncodeCallbackFired;


		class TestClass
		{
			public DateTime date = new DateTime( 2020, 1, 1 );
			public int x;
			public int y;

			[NonSerialized]
			public int z;

			public List<int> list;


			[AfterDecode]
			public void AfterDecode()
			{
				AfterDecodeCallbackFired = true;
			}


			[BeforeEncode]
			public void BeforeDecode()
			{
				BeforeEncodeCallbackFired = true;
			}
		}


		[Test]
		public void DumpClass()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			testClass.list = new List<int> { 3, 1, 4 };

			var json = Nson.ToNson( testClass );
			Assert.AreEqual("Nez.Persistence.NsonTests.ClassTypeTests+TestClass(date:\"2020-01-01T00:00:00Z\",x:5,y:7,list:[3,1,4])", json );

			Assert.IsTrue( BeforeEncodeCallbackFired );
		}

		[Test]
		public void DumpClassPrettyPrint()
		{
			var testClass = new TestClass { x = 5, y = 7, z = 0 };
			testClass.list = new List<int> { 3, 1, 4 };

			var settings = new NsonSettings()
			{
				PrettyPrint = true
			};
            var json = Nson.ToNson(testClass, settings);

            Assert.AreEqual(@"Nez.Persistence.NsonTests.ClassTypeTests+TestClass(
    date: ""2020-01-01T00:00:00Z"",
    x: 5,
    y: 7,
    list: [
        3,
        1,
        4
    ]
)".Replace("    ", "\t"), json);
		}

		[Test]
		public void LoadClass()
        {
            var testClass = Nson.FromNson<TestClass>("Nez.Persistence.NsonTests.ClassTypeTests+TestClass(date:\"2020-01-01T00:00:00Z\",x:5,y:7,list:[3,1,4])");

			Assert.AreEqual( new DateTime( 2020, 1, 1 ), testClass.date );
			Assert.AreEqual( 5, testClass.x );
			Assert.AreEqual( 7, testClass.y );
			Assert.AreEqual( 0, testClass.z ); // should not get assigned

			Assert.AreEqual( 3, testClass.list.Count );
			Assert.AreEqual( 3, testClass.list[0] );
			Assert.AreEqual( 1, testClass.list[1] );
			Assert.AreEqual( 4, testClass.list[2] );

			Assert.IsTrue( AfterDecodeCallbackFired );
		}


		class InnerClass { }

		class OuterClass
		{
			public InnerClass inner;
		}

		[Test]
		public void DumpOuterClassWithNoTypeHintPropagatesToInnerClasses()
		{
			var outerClass = new OuterClass();
			outerClass.inner = new InnerClass();
            var nson = Nson.ToNson(outerClass);

            Assert.AreEqual("Nez.Persistence.NsonTests.ClassTypeTests+OuterClass(inner:Nez.Persistence.NsonTests.ClassTypeTests+InnerClass())", nson );

            var back = Nson.FromNson(nson);
            Assert.IsTrue(back.GetType() == typeof(OuterClass));
		}

	}
}