using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace Nez.Persistence.NsonTests
{
	[TestFixture]
	public class TypeHintAndReferencesTests
	{
		class Entity
		{
			[JsonInclude]
			public bool enabled { get; set; } = true;
			public List<Component> components;

			[BeforeEncode]
			public void BeforeDecode()
			{
                Console.WriteLine( "Entity.BeforeEncode" );
			}

			[AfterDecode]
			public void AfterDecode()
			{
                Console.WriteLine( "Entity.AfterDecode" );
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
			var json = Nson.ToNson( entity, new NsonSettings
			{
				PrettyPrint = true
			} );

			var outEntity = Nson.FromNson<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );


			outEntity = Nson.FromNson<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );
		}

		[Test]
		public void PreserveReferences_Preserves()
		{
			var entity = new Entity();
			entity.components = new List<Component> { new Component(), new Sprite { entity = entity } };

			var json = Nson.ToNson( entity, new NsonSettings
			{
				PrettyPrint = true,
				PreserveReferencesHandling = true
			} );

			var outEntity = Nson.FromNson<Entity>( json );
			Assert.AreEqual( outEntity, outEntity.components[1].entity );
		}

		[Test]
		public void ArrayTypeHint_Hints()
		{
			var entity = new Entity();
			entity.components = new List<Component> { new Component(), new Sprite { entity = entity } };

			var json = Nson.ToNson( entity, new NsonSettings
			{
				PrettyPrint = true,
				PreserveReferencesHandling = true
			} );

			var outEntity = Nson.FromNson<Entity>( json );
			Assert.IsInstanceOf( typeof( Sprite ), outEntity.components[1] );
		}

	}
}