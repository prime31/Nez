using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class BasicTypeEncodingTests
	{
		[Test]
		public void DumpBoolean()
		{
			Assert.AreEqual( "true", Json.ToJson( true ) );
			Assert.AreEqual( "false", Json.ToJson( false ) );
		}

		[Test]
		public void DumpNumber()
		{
			Assert.AreEqual( "12345", Json.ToJson( 12345 ) );
			Assert.AreEqual( "12.34", Json.ToJson( 12.34 ) );
		}

		[Test]
		public void DumpString()
		{
			Assert.AreEqual( "\"string\"", Json.ToJson( "string" ) );
		}

		[Test]
		public void DumpArray()
		{
			Assert.AreEqual( "[1,true,\"three\"]", Json.ToJson( Json.FromJson( "[1,true,\"three\"]" ) ) );
		}

		[Test]
		public void DumpObject()
		{
			Assert.AreEqual( "{\"x\":1,\"y\":2}", Json.ToJson( Json.FromJson( "{\"x\":1,\"y\":2}" ) ) );
		}
	}
}