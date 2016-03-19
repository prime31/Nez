using System;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace MacTester
{
	public class PlayerDashMover : Component, IUpdatable
	{
		float _speed = 800f;
		bool _isGrounded = true;
		bool _destroyedTile;
		Vector2 _moveDir;

		Mover _mover;
		TiledMapComponent _tiledMapComponent;


		public override void onAddedToEntity()
		{
			_tiledMapComponent = entity.scene.findEntity( "tiled-map" ).getComponent<TiledMapComponent>();
			_mover = new Mover();
			entity.addComponent( _mover );
		}


		public void update()
		{
			entity.scene.camera.position = new Vector2( entity.scene.sceneRenderTargetSize.X / 2, entity.scene.sceneRenderTargetSize.Y / 2 );
			if( _isGrounded )
			{
				if( Input.isKeyPressed( Keys.Left ) )
				{
					_moveDir.X = -1f;
					if( !canMove() )
					{
						_moveDir.X = 0;
						return;
					}
					entity.getComponent<RenderableComponent>().flipY = false;
					entity.transform.rotationDegrees = 90f;
				}
				else if( Input.isKeyPressed( Keys.Right ) )
				{
					_moveDir.X = 1f;
					if( !canMove() )
					{
						_moveDir.X = 0;
						return;
					}
					entity.getComponent<RenderableComponent>().flipY = false;
					entity.transform.rotationDegrees = -90f;
				}
				else if( Input.isKeyPressed( Keys.Up ) )
				{
					_moveDir.Y = -1f;
					if( !canMove() )
					{
						_moveDir.Y = 0;
						return;
					}
					entity.getComponent<RenderableComponent>().flipY = true;
					entity.transform.rotationDegrees = 0f;
				}
				else if( Input.isKeyPressed( Keys.Down ) )
				{
					_moveDir.Y = 1f;
					if( !canMove() )
					{
						_moveDir.Y = 0;
						return;
					}
					entity.getComponent<RenderableComponent>().flipY = false;
					entity.transform.rotationDegrees = 0f;
				}
			}


			if( _moveDir != Vector2.Zero )
			{
				var movement = _moveDir * _speed * Time.deltaTime;
				CollisionResult res;
				if( _mover.move( movement, out res ) )
				{
					var pos = entity.transform.position + new Vector2( -16 ) * res.normal;
					var tile = _tiledMapComponent.getTileAtWorldPosition( pos );

					if( !_destroyedTile && tile != null && tile.tilesetTile == null )
					{
						_destroyedTile = true;
						_tiledMapComponent.collisionLayer.removeTile( tile.x, tile.y );
						_tiledMapComponent.removeColliders();
						_tiledMapComponent.addColliders();
					}
					else
					{
						_moveDir = Vector2.Zero;
						_isGrounded = true;
						_destroyedTile = false;
						entity.getComponent<CameraShake>().shake( 8, 0.8f );
					}
				}
				else
				{
					_isGrounded = false;
				}
			}
		}


		void spawnBlock()
		{
			
		}


		bool canMove()
		{
			var pos = entity.transform.position + new Vector2( 16 ) * _moveDir;
			var tile = _tiledMapComponent.getTileAtWorldPosition( pos );

			return tile == null;
		}
	}
}

