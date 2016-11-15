using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// A type of body that supports multiple fixtures that can break apart.
	/// </summary>
	public class BreakableBody
	{
		public bool isBroken;
		public Body mainBody;
		public List<Fixture> parts = new List<Fixture>( 8 );

		/// <summary>
		/// The force needed to break the body apart.
		/// Default: 500
		/// </summary>
		public float Strength = 500.0f;

		float[] _angularVelocitiesCache = new float[8];
		bool _break;
		Vector2[] _velocitiesCache = new Vector2[8];
		World _world;


		public BreakableBody( World world, IEnumerable<Vertices> vertices, float density, Vector2 position = new Vector2(), float rotation = 0 )
		{
			_world = world;
			_world.contactManager.PostSolve += PostSolve;
			mainBody = new Body( _world, position, rotation, BodyType.Dynamic );

			foreach( Vertices part in vertices )
			{
				var polygonShape = new PolygonShape( part, density );
				Fixture fixture = mainBody.CreateFixture( polygonShape );
				parts.Add( fixture );
			}
		}

		public BreakableBody( World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(), float rotation = 0 )
		{
			_world = world;
			_world.contactManager.PostSolve += PostSolve;
			mainBody = new Body( _world, position, rotation, BodyType.Dynamic );

			foreach( Shape part in shapes )
			{
				Fixture fixture = mainBody.CreateFixture( part );
				parts.Add( fixture );
			}
		}

		void PostSolve( Contact contact, ContactVelocityConstraint impulse )
		{
			if( !isBroken )
			{
				if( parts.Contains( contact.FixtureA ) || parts.Contains( contact.FixtureB ) )
				{
					float maxImpulse = 0.0f;
					int count = contact.Manifold.PointCount;

					for( int i = 0; i < count; ++i )
					{
						maxImpulse = Math.Max( maxImpulse, impulse.points[i].normalImpulse );
					}

					if( maxImpulse > Strength )
					{
						// Flag the body for breaking.
						_break = true;
					}
				}
			}
		}

		public void Update()
		{
			if( _break )
			{
				Decompose();
				isBroken = true;
				_break = false;
			}

			// Cache velocities to improve movement on breakage.
			if( isBroken == false )
			{
				//Enlarge the cache if needed
				if( parts.Count > _angularVelocitiesCache.Length )
				{
					_velocitiesCache = new Vector2[parts.Count];
					_angularVelocitiesCache = new float[parts.Count];
				}

				//Cache the linear and angular velocities.
				for( int i = 0; i < parts.Count; i++ )
				{
					_velocitiesCache[i] = parts[i].body.linearVelocity;
					_angularVelocitiesCache[i] = parts[i].body.angularVelocity;
				}
			}
		}

		void Decompose()
		{
			//Unsubsribe from the PostSolve delegate
			_world.contactManager.PostSolve -= PostSolve;

			for( int i = 0; i < parts.Count; i++ )
			{
				var oldFixture = parts[i];

				Shape shape = oldFixture.shape.Clone();
				object userData = oldFixture.userData;

				mainBody.DestroyFixture( oldFixture );

				Body body = BodyFactory.CreateBody( _world, mainBody.position, mainBody.rotation, BodyType.Dynamic, mainBody.userData );

				var newFixture = body.CreateFixture( shape );
				newFixture.userData = userData;
				parts[i] = newFixture;

				body.angularVelocity = _angularVelocitiesCache[i];
				body.linearVelocity = _velocitiesCache[i];
			}

			_world.RemoveBody( mainBody );
			_world.RemoveBreakableBody( this );
		}

		public void Break()
		{
			_break = true;
		}
	
	}
}