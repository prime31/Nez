using Nez.Persistence;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class EnforceHeirarchyOrderTests
	{
		class ClassA
		{
			public int FieldA;
			public int PropertyA { get; set; }
		}

		class ClassB : ClassA
		{
			public int FieldB;
			public int PropertyB { get; set; }
		}

		class ClassC : ClassB
		{
			public int FieldC;
			public int PropertyC { get; set; }
		}


		[Test]
		public void TestEncodeWithEnforceHeirarchyOrderEnabled()
		{
			var settings = new JsonSettings()
			{
				EnforceHeirarchyOrderEnabled = true
			};
			var testClass = new ClassC { FieldA = 1, FieldB = 2, FieldC = 3, PropertyA = 4, PropertyB = 5, PropertyC = 6 };
			var json = Json.Encode( testClass, settings );
			Assert.AreEqual( "{\"FieldA\":1,\"FieldB\":2,\"FieldC\":3,\"PropertyA\":4,\"PropertyB\":5,\"PropertyC\":6}", json );
		}
	}
}