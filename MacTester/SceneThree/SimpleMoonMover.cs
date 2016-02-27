using System;
using Nez;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleMoonMover : Component, IUpdatable
	{
		float _speed = 10f;
		Mover _mover;


		public override void onAddedToEntity()
		{
			_mover = new Mover();
			entity.addComponent( _mover );
		}


		public void update()
		{
			var moveDir = Vector2.Zero;

			if( Input.isKeyDown( Keys.Left ) )
			{
				moveDir.X = -1f;
				entity.getComponent<RenderableComponent>().flipX = true;
			}
			else if( Input.isKeyDown( Keys.Right ) )
			{
				moveDir.X = 1f;
				entity.getComponent<RenderableComponent>().flipX = false;
			}

			if( Input.isKeyDown( Keys.Up ) )
				moveDir.Y = -1f;
			else if( Input.isKeyDown( Keys.Down ) )
				moveDir.Y = 1f;


			if( moveDir != Vector2.Zero )
			{
				var movement = moveDir * _speed;

				CollisionResult res;
				if( _mover.move( movement, out res ) )
					Debug.drawLine( entity.transform.position, entity.transform.position + res.normal * 100, Color.Black, 0.3f );
			}
		}
	}
}

