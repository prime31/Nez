using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

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

			[JsonExclude]
			public int totalOrphanedKeys;
			[JsonExclude]
			public bool wasCreatedByObjectFactory;
		}

		class DoodleContainer
		{
			public Doodle firstDoodle;
			public Doodle secondDoodle;
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
				System.Console.WriteLine( $"field name: {key}, value: {value}" );
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
			{ }
		}

		class ObjectFactoryConverter : JsonObjectFactory<Doodle>
		{
			public override Doodle Create( Type objectType, IDictionary objectData )
			{
				var doodle = new Doodle
				{
					wasCreatedByObjectFactory = true
				};

				doodle.x = Convert.ToInt32( objectData["x"] );
				doodle.y = Convert.ToInt32( objectData["y"] );
				doodle.z = Convert.ToInt32( objectData["z"] );

				return doodle;
			}
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

		[Test]
		public void Converter_ObjectFactory()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newDoodle = Json.FromJson<Doodle>( json, settings );

			Assert.IsTrue( newDoodle.wasCreatedByObjectFactory );

			Assert.AreEqual( newDoodle.x, doodle.x );
			Assert.AreEqual( newDoodle.y, doodle.y );
			Assert.AreEqual( newDoodle.z, doodle.z );
		}

		[Test]
		public void Converter_ObjectFactoryNested()
		{
			var container = new DoodleContainer
			{
				firstDoodle = new Doodle { x = 1, y = 2, z = 3 },
				secondDoodle = new Doodle { x = 4, y = 5, z = 5 }
			};
			var json = Json.ToJson( container );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newContainer = Json.FromJson<DoodleContainer>( json, settings );

			Assert.IsTrue( newContainer.firstDoodle.wasCreatedByObjectFactory );
			Assert.IsTrue( newContainer.secondDoodle.wasCreatedByObjectFactory );

			Assert.AreEqual( container.firstDoodle.x, newContainer.firstDoodle.x );
			Assert.AreEqual( container.firstDoodle.y, newContainer.firstDoodle.y );
			Assert.AreEqual( container.firstDoodle.z, newContainer.firstDoodle.z );

			Assert.AreEqual( container.secondDoodle.x, newContainer.secondDoodle.x );
			Assert.AreEqual( container.secondDoodle.y, newContainer.secondDoodle.y );
			Assert.AreEqual( container.secondDoodle.z, newContainer.secondDoodle.z );
		}

		[Test]
		public void Converter_ObjectFactoryList()
		{
			var list = new List<Doodle>
			{
				{ new Doodle { x = 1, y = 2, z = 3 } },
				{ new Doodle { x = 4, y = 5, z = 5 } }
			};
			var json = Json.ToJson( list );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newList = Json.FromJson<List<Doodle>>( json, settings );

			Assert.IsTrue( newList[0].wasCreatedByObjectFactory );
			Assert.IsTrue( newList[1].wasCreatedByObjectFactory );

			Assert.AreEqual( list[0].x, newList[0].x );
			Assert.AreEqual( list[0].y, newList[0].y );
			Assert.AreEqual( list[0].z, newList[0].z );

			Assert.AreEqual( list[1].x, newList[1].x );
			Assert.AreEqual( list[1].y, newList[1].y );
			Assert.AreEqual( list[1].z, newList[1].z );
		}

		[Test]
		public void Converter_ObjectFactoryWithTypeHint()
		{
			var doodle = new Doodle { x = 5, y = 7, z = 9 };
			var json = Json.ToJson( doodle, new JsonSettings { TypeNameHandling = TypeNameHandling.All } );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newDoodle = Json.FromJson<Doodle>( json, settings );

			Assert.IsTrue( newDoodle.wasCreatedByObjectFactory );

			Assert.AreEqual( newDoodle.x, doodle.x );
			Assert.AreEqual( newDoodle.y, doodle.y );
			Assert.AreEqual( newDoodle.z, doodle.z );
		}

		[Test]
		public void Converter_ObjectFactoryNestedWithTypeHint()
		{
			var container = new DoodleContainer
			{
				firstDoodle = new Doodle { x = 1, y = 2, z = 3 },
				secondDoodle = new Doodle { x = 4, y = 5, z = 5 }
			};
			var json = Json.ToJson( container, new JsonSettings { TypeNameHandling = TypeNameHandling.All } );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newContainer = Json.FromJson<DoodleContainer>( json, settings );

			Assert.IsTrue( newContainer.firstDoodle.wasCreatedByObjectFactory );
			Assert.IsTrue( newContainer.secondDoodle.wasCreatedByObjectFactory );

			Assert.AreEqual( container.firstDoodle.x, newContainer.firstDoodle.x );
			Assert.AreEqual( container.firstDoodle.y, newContainer.firstDoodle.y );
			Assert.AreEqual( container.firstDoodle.z, newContainer.firstDoodle.z );

			Assert.AreEqual( container.secondDoodle.x, newContainer.secondDoodle.x );
			Assert.AreEqual( container.secondDoodle.y, newContainer.secondDoodle.y );
			Assert.AreEqual( container.secondDoodle.z, newContainer.secondDoodle.z );
		}

		[Test]
		public void Converter_ObjectFactoryListWithTypeHint()
		{
			var list = new List<Doodle>
			{
				{ new Doodle { x = 1, y = 2, z = 3 } },
				{ new Doodle { x = 4, y = 5, z = 5 } }
			};
			var json = Json.ToJson( list, new JsonSettings { TypeNameHandling = TypeNameHandling.Arrays } );

			var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
			var newList = Json.FromJson<List<Doodle>>( json, settings );

			Assert.IsTrue( newList[0].wasCreatedByObjectFactory );
			Assert.IsTrue( newList[1].wasCreatedByObjectFactory );

			Assert.AreEqual( list[0].x, newList[0].x );
			Assert.AreEqual( list[0].y, newList[0].y );
			Assert.AreEqual( list[0].z, newList[0].z );

			Assert.AreEqual( list[1].x, newList[1].x );
			Assert.AreEqual( list[1].y, newList[1].y );
			Assert.AreEqual( list[1].z, newList[1].z );
		}

	}
}