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

