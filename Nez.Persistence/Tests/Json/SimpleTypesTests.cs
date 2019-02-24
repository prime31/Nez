using System;
using System.Collections.Generic;
using System.Globalization;
using Nez.Persistence;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class SimpleTypesTests
	{
		[Test]
		public void TestDumpBool()
		{
			Assert.AreEqual( "true", Json.ToJson( true ) );
			Assert.AreEqual( "false", Json.ToJson( false ) );
		}


		[Test]
		public void TestLoadBool()
		{
			Assert.AreEqual( true, (Boolean)Json.FromJson( "true" ) );
			Assert.AreEqual( false, (Boolean)Json.FromJson( "false" ) );
		}


		[Test]
		public void TestDumpIntegerTypes()
		{
			Assert.AreEqual( "-12345", Json.ToJson( (Int16)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.ToJson( (Int32)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.ToJson( (Int64)( -12345 ) ) );

			Assert.AreEqual( "12345", Json.ToJson( (UInt16)12345 ) );
			Assert.AreEqual( "12345", Json.ToJson( (UInt32)12345 ) );
			Assert.AreEqual( "12345", Json.ToJson( (UInt64)12345 ) );
		}


		[Test]
		public void TestLoadIntegerTypes()
		{
			Assert.AreEqual( -12345, (Int32)Json.FromJson( "-12345" ) );
		}


		[Test]
		public void TestDumpFloatTypes()
		{
			Assert.AreEqual( "123.45", Json.ToJson( (Single)123.45 ) );
			Assert.AreEqual( "123.45", Json.ToJson( (Double)123.45 ) );
		}


		[Test]
		public void TestDumpFloatTypesForGermanCulture()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = new CultureInfo( "de", false );
			Assert.AreEqual( "123.45", Json.ToJson( (Single)123.45 ) );
			Assert.AreEqual( "123.45", Json.ToJson( (Double)123.45 ) );
			CultureInfo.CurrentCulture = currentCulture;
		}


		[Test]
		public void TestDumpDecimalType()
		{
			Assert.AreEqual( "79228162514264337593543950335", Json.ToJson( Decimal.MaxValue ) );
			Assert.AreEqual( "-79228162514264337593543950335", Json.ToJson( Decimal.MinValue ) );
		}


		[Test]
		public void TestLoadFloatTypes()
		{
			Assert.AreEqual( 123.45f, (Single)Json.FromJson( "123.45" ) );
		}


		[Test]
		public void TestDumpString()
		{
			Assert.AreEqual( "\"OHAI! Can haz ball of strings?\"", Json.ToJson( "OHAI! Can haz ball of strings?" ) );
			Assert.AreEqual( "\"\\\"\"", Json.ToJson( "\"" ) );
			Assert.AreEqual( "\"\\\\\"", Json.ToJson( "\\" ) );
			Assert.AreEqual( "\"\\b\"", Json.ToJson( "\b" ) );
			Assert.AreEqual( "\"\\f\"", Json.ToJson( "\f" ) );
			Assert.AreEqual( "\"\\n\"", Json.ToJson( "\n" ) );
			Assert.AreEqual( "\"\\r\"", Json.ToJson( "\r" ) );
			Assert.AreEqual( "\"\\t\"", Json.ToJson( "\t" ) );
			Assert.AreEqual( "\"c\"", Json.ToJson( 'c' ) );
		}


		[Test]
		public void TestLoadString()
		{
			Assert.AreEqual( "OHAI! Can haz ball of strings?", (String)Json.FromJson( "\"OHAI! Can haz ball of strings?\"" ) );
			Assert.AreEqual( "\"", (String)Json.FromJson( "\"\\\"\"" ) );
			Assert.AreEqual( "\\", (String)Json.FromJson( "\"\\\\\"" ) );
			Assert.AreEqual( "\b", (String)Json.FromJson( "\"\\b\"" ) );
			Assert.AreEqual( "\f", (String)Json.FromJson( "\"\\f\"" ) );
			Assert.AreEqual( "\n", (String)Json.FromJson( "\"\\n\"" ) );
			Assert.AreEqual( "\r", (String)Json.FromJson( "\"\\r\"" ) );
			Assert.AreEqual( "\t", (String)Json.FromJson( "\"\\t\"" ) );
		}


		[Test]
		public void TestDumpNull()
		{
			List<int> list = null;
			Assert.AreEqual( "null", Json.ToJson( list ) );
			Assert.AreEqual( "null", Json.ToJson( null ) );
		}


		[Test]
		public void TestLoadNull()
		{
			Assert.AreEqual( null, Json.FromJson( "null" ) );
		}


		class ValueTypes
		{
			public Int16 i16 = 1;
			public UInt16 u16 = 2;
			public Int32 i32 = 3;
			public UInt32 u32 = 4;
			public Int64 i64 = 5;
			public UInt64 u64 = 6;
			public Single s = 7;
			public Double d = 8;
			public Decimal m = 9;
			public Boolean b = true;
		}


		[Test]
		public void TestAOTCompatibility()
		{
			ValueTypes item = new ValueTypes();
			const string json = "{\"i16\":1,\"u16\":2,\"i32\":3,\"u32\":4,\"i64\":5,\"u64\":6,\"s\":7,\"d\":8,\"m\":9,\"b\":true}";
			var data = Json.FromJson( json );
			Assert.DoesNotThrow( () => VariantConverter.Decode<ValueTypes>( data ) );
			Assert.DoesNotThrow( () => VariantConverter.DecodeInto( data, out item ) );
		}
	}
}