using Nez.Persistence;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class VariantEncodingTests
	{
		[Test]
		public void DumpProxyBoolean()
		{
			Assert.AreEqual( "true", new ProxyBoolean( true ).ToJson() );
			Assert.AreEqual( "false", new ProxyBoolean( false ).ToJson() );
		}


		[Test]
		public void DumpProxyNumber()
		{
			Assert.AreEqual( "12345", new ProxyNumber( 12345 ).ToJson() );
			Assert.AreEqual( "12.34", new ProxyNumber( 12.34 ).ToJson() );
		}


		[Test]
		public void DumpProxyString()
		{
			Assert.AreEqual( "\"string\"", new ProxyString( "string" ).ToJson() );
		}


		[Test]
		public void DumpProxyArray()
		{
			Assert.AreEqual( "[1,true,\"three\"]", Json.Decode( "[1,true,\"three\"]" ).ToJson() );
		}


		[Test]
		public void DumpProxyObject()
		{
			Assert.AreEqual( "{\"x\":1,\"y\":2}", Json.Decode( "{\"x\":1,\"y\":2}" ).ToJson() );
		}
	}
}