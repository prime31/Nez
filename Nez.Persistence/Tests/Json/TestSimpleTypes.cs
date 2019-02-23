using System;
using System.Collections.Generic;
using System.Globalization;
using Nez.Persistance;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestSimpleTypes
	{
		[Test]
		public void TestDumpBool()
		{
			Assert.AreEqual( "true", Json.Encode( true ) );
			Assert.AreEqual( "false", Json.Encode( false ) );
		}


		[Test]
		public void TestLoadBool()
		{
			Assert.AreEqual( true, (Boolean)Json.Decode( "true" ) );
			Assert.AreEqual( false, (Boolean)Json.Decode( "false" ) );
		}


		[Test]
		public void TestDumpIntegerTypes()
		{
			Assert.AreEqual( "-12345", Json.Encode( (Int16)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.Encode( (Int32)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.Encode( (Int64)( -12345 ) ) );

			Assert.AreEqual( "12345", Json.Encode( (UInt16)12345 ) );
			Assert.AreEqual( "12345", Json.Encode( (UInt32)12345 ) );
			Assert.AreEqual( "12345", Json.Encode( (UInt64)12345 ) );
		}


		[Test]
		public void TestLoadIntegerTypes()
		{
			Assert.AreEqual( -12345, (Int32)Json.Decode( "-12345" ) );
		}


		[Test]
		public void TestDumpFloatTypes()
		{
			Assert.AreEqual( "123.45", Json.Encode( (Single)123.45 ) );
			Assert.AreEqual( "123.45", Json.Encode( (Double)123.45 ) );
		}


		[Test]
		public void TestDumpFloatTypesForGermanCulture()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = new CultureInfo( "de", false );
			Assert.AreEqual( "123.45", Json.Encode( (Single)123.45 ) );
			Assert.AreEqual( "123.45", Json.Encode( (Double)123.45 ) );
			CultureInfo.CurrentCulture = currentCulture;
		}


		[Test]
		public void TestDumpDecimalType()
		{
			Assert.AreEqual( "79228162514264337593543950335", Json.Encode( Decimal.MaxValue ) );
			Assert.AreEqual( "-79228162514264337593543950335", Json.Encode( Decimal.MinValue ) );
		}


		[Test]
		public void TestLoadFloatTypes()
		{
			Assert.AreEqual( 123.45f, (Single)Json.Decode( "123.45" ) );
		}


		[Test]
		public void TestDumpString()
		{
			Assert.AreEqual( "\"OHAI! Can haz ball of strings?\"", Json.Encode( "OHAI! Can haz ball of strings?" ) );
			Assert.AreEqual( "\"\\\"\"", Json.Encode( "\"" ) );
			Assert.AreEqual( "\"\\\\\"", Json.Encode( "\\" ) );
			Assert.AreEqual( "\"\\b\"", Json.Encode( "\b" ) );
			Assert.AreEqual( "\"\\f\"", Json.Encode( "\f" ) );
			Assert.AreEqual( "\"\\n\"", Json.Encode( "\n" ) );
			Assert.AreEqual( "\"\\r\"", Json.Encode( "\r" ) );
			Assert.AreEqual( "\"\\t\"", Json.Encode( "\t" ) );
			Assert.AreEqual( "\"c\"", Json.Encode( 'c' ) );
		}


		[Test]
		public void TestLoadString()
		{
			Assert.AreEqual( "OHAI! Can haz ball of strings?", (String)Json.Decode( "\"OHAI! Can haz ball of strings?\"" ) );
			Assert.AreEqual( "\"", (String)Json.Decode( "\"\\\"\"" ) );
			Assert.AreEqual( "\\", (String)Json.Decode( "\"\\\\\"" ) );
			Assert.AreEqual( "\b", (String)Json.Decode( "\"\\b\"" ) );
			Assert.AreEqual( "\f", (String)Json.Decode( "\"\\f\"" ) );
			Assert.AreEqual( "\n", (String)Json.Decode( "\"\\n\"" ) );
			Assert.AreEqual( "\r", (String)Json.Decode( "\"\\r\"" ) );
			Assert.AreEqual( "\t", (String)Json.Decode( "\"\\t\"" ) );
		}


		[Test]
		public void TestDumpNull()
		{
			List<int> list = null;
			Assert.AreEqual( "null", Json.Encode( list ) );
			Assert.AreEqual( "null", Json.Encode( null ) );
		}


		[Test]
		public void TestLoadNull()
		{
			Assert.AreEqual( null, Json.Decode( "null" ) );
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
			var data = Json.Decode( json );
			Assert.DoesNotThrow( () => VariantConverter.Make<ValueTypes>( data ) );
			Assert.DoesNotThrow( () => VariantConverter.MakeInto( data, out item ) );
		}
	}
}