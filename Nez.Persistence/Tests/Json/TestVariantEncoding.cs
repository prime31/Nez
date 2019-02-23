using Nez.Persistance;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestVariantEncoding
	{
		[Test]
		public void TestDumpProxyBoolean()
		{
			Assert.AreEqual( "true", new ProxyBoolean( true ).ToJson() );
			Assert.AreEqual( "false", new ProxyBoolean( false ).ToJson() );
		}


		[Test]
		public void TestDumpProxyNumber()
		{
			Assert.AreEqual( "12345", new ProxyNumber( 12345 ).ToJson() );
			Assert.AreEqual( "12.34", new ProxyNumber( 12.34 ).ToJson() );
		}


		[Test]
		public void TestDumpProxyString()
		{
			Assert.AreEqual( "\"string\"", new ProxyString( "string" ).ToJson() );
		}


		[Test]
		public void TestDumpProxyArray()
		{
			Assert.AreEqual( "[1,true,\"three\"]", Json.Decode( "[1,true,\"three\"]" ).ToJson() );
		}


		[Test]
		public void TestDumpProxyObject()
		{
			Assert.AreEqual( "{\"x\":1,\"y\":2}", Json.Decode( "{\"x\":1,\"y\":2}" ).ToJson() );
		}
	}
}