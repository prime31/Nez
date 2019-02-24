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
		}

		class TrueJsonConverter : JsonTypeConverter<Doodle>
		{
			public override Doodle ConvertToObject( IObjectConverter converter, Type objectType, Doodle existingValue, ProxyObject data )
			{
				var doodle = new Doodle();
				foreach( var bits in data )
				{
					Debug.log( $"field name: {bits.Key}, value: {bits.Value}" );
				}

				return doodle;
			}

			public override void WriteJson( IJsonEncoder encoder, Doodle value )
			{
				encoder.EncodeValue( true );
			}
		}


		[Test]
		public void Converter_CanWriteJson()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, new TrueJsonConverter() );

			Assert.AreEqual( "true", json );
		}


		[Test]
		public void Converter_CanConvertToObject()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new TrueJsonConverter() } };
			var newDoodle = Json.FromJson<Doodle>( json, settings );

			Assert.AreNotEqual( doodle.x, newDoodle.x );
			Assert.AreNotEqual( doodle.y, newDoodle.y );
			Assert.AreNotEqual( doodle.z, newDoodle.z );
		}
	}
}