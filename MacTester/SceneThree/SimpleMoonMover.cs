using System;
using Nez;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleMoonMover : Component
	{
		float _speed = 10f;

		
		public override void update()
		{
			var moveDir = Vector2.Zero;

			if( Input.getKeyDown( Keys.Left ) )
				moveDir.X = -1f;
			else if( Input.getKeyDown( Keys.Right ) )
				moveDir.X = 1f;

			if( Input.getKeyDown( Keys.Up ) )
				moveDir.Y = -1f;
			else if( Input.getKeyDown( Keys.Down ) )
				moveDir.Y = 1f;

			entity.move( moveDir.X * _speed, moveDir.Y * _speed );
		}
	}
}

