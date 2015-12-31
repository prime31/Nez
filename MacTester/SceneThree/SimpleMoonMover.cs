using System;
using Nez;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleMoonMover : Component, ICollisionCallback, ITriggerCallback
	{
		float _speed = 10f;

		
		public override void update()
		{
			var moveDir = Vector2.Zero;

			if( Input.isKeyDown( Keys.Left ) )
				moveDir.X = -1f;
			else if( Input.isKeyDown( Keys.Right ) )
				moveDir.X = 1f;

			if( Input.isKeyDown( Keys.Up ) )
				moveDir.Y = -1f;
			else if( Input.isKeyDown( Keys.Down ) )
				moveDir.Y = 1f;


			if( moveDir != Vector2.Zero )
			{
				var movement = moveDir * _speed;
				entity.moveActor( movement );
			}
		}


		void move( Vector2 movement )
		{
			Physics.removeCollider( entity.collider, true );

			var neighbors = Physics.boxcastBroadphaseExcludingSelf( entity.collider, movement.X, movement.Y );
			foreach( var neighbor in neighbors )
			{
				neighbor.shape.position = neighbor.entity.position;
				ShapeCollisionResult result;
				entity.collider.shape.position = entity.position + movement;

				if( entity.collider.shape.collidesWithShape( neighbor.shape, out result ) )
				{
					movement -= result.minimumTranslationVector;
					Debug.log( "collided result: {0}. new movement: {1}", result, movement );
				}
			}

			entity.position += movement;

			Physics.addCollider( entity.collider );
		}


		public void onCollisionEnter( Collider collider, CollisionDirection direction )
		{
			Debug.log( "Collided with {0} direction: {1}", collider.entity, direction );
		}


		public void onTriggerEnter( Collider other )
		{
			Debug.log( "Triggered with {0}", other.entity );
		}

	}
}

