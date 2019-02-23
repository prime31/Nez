using Nez.Persistance;
using NUnit.Framework;
using System.Collections.Generic;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestTypeHintAndReferences
	{
		class Entity
		{
			public bool enabled { get; set; } = true;
			public List<Component> components;

			[BeforeEncodeAttribute]
			public void BeforeDecode()
			{
				System.Console.WriteLine( "Entity.BeforeEncode" );
			}

			[AfterDecodeAttribute]
			public void AfterDecode()
			{
				System.Console.WriteLine( "Entity.AfterDecode" );
			}
		}

		class Component
		{
			public Entity entity;
			public int index;
		}

		class Sprite : Component
		{ }


		[Test]
		public void TestTypeHintAuto()
		{
			var entity = new Entity
			{
				components = new List<Component> { new Component(), new Sprite() }
			};
			var json = Json.Encode( entity, new JsonSettings
			{
				PrettyPrint = true,
				TypeNameHandling = TypeNameHandling.Auto
			} );

			var outEntity = Json.Decode<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );
		}

		[Test]
		public void TestPreserveReferences()
		{
			var entity = new Entity();
			entity.components = new List<Component> { new Component(), new Sprite() { entity = entity } };

			var json = Json.Encode( entity, new JsonSettings
			{
				PrettyPrint = true,
				TypeNameHandling = TypeNameHandling.Auto,
				PreserveReferencesHandling = PreserveReferencesHandling.All
			} );

			var outEntity = Json.Decode<Entity>( json );
			Assert.AreEqual( outEntity, outEntity.components[1].entity );
		}

	}
}