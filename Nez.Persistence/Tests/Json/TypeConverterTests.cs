using NUnit.Framework;
using System;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TypeConverterTests
	{
		class Doodle
		{
			public int x;
			public int y;
			public int z;

			[NonSerialized]
			public int totalOrphanedKeys;
		}

		class CustomDataConverter : JsonTypeConverter<Doodle>
		{
			public override void WriteJson( IJsonEncoder encoder, Doodle value )
			{
				encoder.EncodeKeyValuePair( "key-that-isnt-on-object", true );
				encoder.EncodeKeyValuePair( "another_key", "with a value" );
			}

			public override void OnFoundCustomData( Doodle instance, string key, object value )
			{
				instance.totalOrphanedKeys++;
				Debug.log( $"field name: {key}, value: {value}" );
			}
		}

		class WantsExclusiveWriteConverter : JsonTypeConverter<Doodle>
		{
			public override bool WantsExclusiveWrite => true;

			public override void WriteJson( IJsonEncoder encoder, Doodle value )
			{
				encoder.EncodeKeyValuePair( "key-that-isnt-on-object", true );
				encoder.EncodeKeyValuePair( "another_key", "with a value" );
				encoder.EncodeKeyValuePair( "string_array", new string[] { "first", "second" } );
			}

			public override void OnFoundCustomData( Doodle instance, string key, object value )
			{}
		}


		[Test]
		public void Converter_WriteJson()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, new CustomDataConverter() );

			Assert.IsTrue( json.Contains( "key-that-isnt-on-object" ) );
			Assert.IsTrue( json.Contains( "another_key" ) );
		}

		[Test]
		public void Converter_OnFoundCustomData()
		{
			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new CustomDataConverter() } };
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, settings );

			var newDoodle = Json.FromJson<Doodle>( json, settings );

			Assert.AreEqual( 2, newDoodle.totalOrphanedKeys );
			Assert.AreNotEqual( doodle.totalOrphanedKeys, newDoodle.totalOrphanedKeys );
		}

		[Test]
		public void Converter_WantsExclusiveWrite()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, new WantsExclusiveWriteConverter() );

			Assert.IsTrue( json.Contains( "key-that-isnt-on-object" ) );
			Assert.IsTrue( json.Contains( "another_key" ) );
			Assert.IsTrue( json.Contains( "string_array" ) );
			Assert.IsFalse( json.Contains( "x" ) );
		}
	}
}