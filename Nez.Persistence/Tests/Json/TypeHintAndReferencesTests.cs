using NUnit.Framework;
using System.Collections.Generic;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TypeHintAndReferencesTests
	{
		class Entity
		{
			[Serialized]
			public bool enabled { get; set; } = true;
			public List<Component> components;

			[BeforeEncode]
			public void BeforeDecode()
			{
				System.Console.WriteLine( "Entity.BeforeEncode" );
			}

			[AfterDecode]
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
		public void TypeHintAuto()
		{
			var entity = new Entity
			{
				components = new List<Component> { new Component(), new Sprite() }
			};
			var json = Json.ToJson( entity, new JsonSettings
			{
				PrettyPrint = true,
				TypeNameHandling = TypeNameHandling.Auto
			} );

			var outEntity = Json.FromJson<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );


			outEntity = JsonDirectDecoder.FromJson<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );
		}

		[Test]
		public void PreserveReferences()
		{
			var entity = new Entity();
			entity.components = new List<Component> { new Component(), new Sprite { entity = entity } };

			var json = Json.ToJson( entity, new JsonSettings
			{
				PrettyPrint = true,
				TypeNameHandling = TypeNameHandling.Auto,
				PreserveReferencesHandling = true
			} );

			var outEntity = Json.FromJson<Entity>( json );
			Assert.AreEqual( outEntity, outEntity.components[1].entity );


			outEntity = JsonDirectDecoder.FromJson<Entity>( json );
			Assert.AreEqual( outEntity, outEntity.components[1].entity );
		}

	}
}