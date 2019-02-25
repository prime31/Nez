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

		class TrueJsonConverter : JsonTypeConverter<Doodle>
		{
			public override void WriteJson( IJsonEncoder encoder, Doodle value )
			{
				encoder.WriteValueDelimiter();
				encoder.WriteString( "key-that-isnt-on-object" );
				encoder.AppendColon();
				encoder.EncodeValue( true );

				encoder.WriteValueDelimiter();
				encoder.WriteString( "another_key" );
				encoder.AppendColon();
				encoder.EncodeValue( "with a value" );
			}

			public override void OnFoundCustomData( Doodle instance, string key, object value )
			{
				instance.totalOrphanedKeys++;
				Debug.log( $"field name: {key}, value: {value}" );
			}
		}


		[Test]
		public void Converter_WriteJson()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, new TrueJsonConverter() );

			Assert.IsTrue( json.Contains( "key-that-isnt-on-object" ) );
			Assert.IsTrue( json.Contains( "another_key" ) );
		}


		[Test]
		public void Converter_OnFoundCustomData()
		{
			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new TrueJsonConverter() } };
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, settings );

			var newDoodle = JsonDirectDecoder.FromJson<Doodle>( json, settings );

			Assert.AreEqual( 2, newDoodle.totalOrphanedKeys );
			Assert.AreNotEqual( doodle.totalOrphanedKeys, newDoodle.totalOrphanedKeys );
		}
	}
}