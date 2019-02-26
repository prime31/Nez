using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class SimpleTypesTests
	{
		[Test]
		public void DumpBool()
		{
			Assert.AreEqual( "true", Json.ToJson( true ) );
			Assert.AreEqual( "false", Json.ToJson( false ) );
		}


		[Test]
		public void LoadBool()
		{
			Assert.AreEqual( true, (bool)Json.FromJson( "true" ) );
			Assert.AreEqual( false, (bool)Json.FromJson( "false" ) );
		}


		[Test]
		public void DumpIntegerTypes()
		{
			Assert.AreEqual( "-12345", Json.ToJson( (short)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.ToJson( (int)( -12345 ) ) );
			Assert.AreEqual( "-12345", Json.ToJson( (long)( -12345 ) ) );

			Assert.AreEqual( "12345", Json.ToJson( (ushort)12345 ) );
			Assert.AreEqual( "12345", Json.ToJson( (uint)12345 ) );
			Assert.AreEqual( "12345", Json.ToJson( (ulong)12345 ) );
		}


		[Test]
		public void LoadIntegerTypes()
		{
			Assert.AreEqual( -12345, Json.FromJson<int>( "-12345" ) );
		}


		[Test]
		public void DumpFloatTypes()
		{
			Assert.AreEqual( "123.45", Json.ToJson( (float)123.45 ) );
			Assert.AreEqual( "123.45", Json.ToJson( (double)123.45 ) );
		}


		[Test]
		public void DumpFloatTypesForGermanCulture()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = new CultureInfo( "de", false );
			Assert.AreEqual( "123.45", Json.ToJson( (float)123.45 ) );
			Assert.AreEqual( "123.45", Json.ToJson( (double)123.45 ) );
			CultureInfo.CurrentCulture = currentCulture;
		}


		[Test]
		public void DumpDecimalType()
		{
			Assert.AreEqual( "79228162514264337593543950335", Json.ToJson( decimal.MaxValue ) );
			Assert.AreEqual( "-79228162514264337593543950335", Json.ToJson( decimal.MinValue ) );
		}


		[Test]
		public void LoadFloatTypes()
		{
			Assert.AreEqual( 123.45f, Json.FromJson<float>( "123.45" ) );
		}


		[Test]
		public void DumpString()
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
		public void LoadString()
		{
			Assert.AreEqual( "OHAI! Can haz ball of strings?", (string)Json.FromJson( "\"OHAI! Can haz ball of strings?\"" ) );
			Assert.AreEqual( "\"", (string)Json.FromJson( "\"\\\"\"" ) );
			Assert.AreEqual( "\\", (string)Json.FromJson( "\"\\\\\"" ) );
			Assert.AreEqual( "\b", (string)Json.FromJson( "\"\\b\"" ) );
			Assert.AreEqual( "\f", (string)Json.FromJson( "\"\\f\"" ) );
			Assert.AreEqual( "\n", (string)Json.FromJson( "\"\\n\"" ) );
			Assert.AreEqual( "\r", (string)Json.FromJson( "\"\\r\"" ) );
			Assert.AreEqual( "\t", (string)Json.FromJson( "\"\\t\"" ) );
		}


		[Test]
		public void DumpNull()
		{
			List<int> list = null;
			Assert.AreEqual( "null", Json.ToJson( list ) );
			Assert.AreEqual( "null", Json.ToJson( null ) );
		}


		[Test]
		public void LoadNull()
		{
			Assert.AreEqual( null, Json.FromJson( "null" ) );
		}


		class ValueTypes
		{
			public short i16 = 1;
			public ushort u16 = 2;
			public int i32 = 3;
			public uint u32 = 4;
			public long i64 = 5;
			public ulong u64 = 6;
			public float s = 7;
			public double d = 8;
			public decimal m = 9;
			public bool b = true;
		}


		[Test]
		public void AOTCompatibility()
		{
			var item = new ValueTypes();
			const string json = "{\"i16\":1,\"u16\":2,\"i32\":3,\"u32\":4,\"i64\":5,\"u64\":6,\"s\":7,\"d\":8,\"m\":9,\"b\":true}";

			Assert.DoesNotThrow( () => Json.FromJson<ValueTypes>( json ) );
			Assert.DoesNotThrow( () => Json.FromJsonOverwrite( json, item ) );
		}
	}
}