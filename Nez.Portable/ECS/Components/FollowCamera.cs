using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// basic follow camera. LockOn mode uses no deadzone and just centers the camera on the target. CameraWindow mode wraps a deadzone
	/// around the target allowing it to move within the deadzone without moving the camera.
	/// </summary>
	public class FollowCamera : Component, IUpdatable
	{
		public enum CameraStyle
		{
			LockOn,
			CameraWindow
		}
			
		public Camera camera;

		/// <summary>
		/// how fast the camera closes the distance to the target position
		/// </summary>
		public float followLerp = 0.2f;

		/// <summary>
		/// when in CameraWindow mode the width/height is used as a bounding box to allow movement within it without moving the camera.
		/// when in LockOn mode only the deadzone x/y values are used. This is set to sensible defaults when you call follow but you are
		/// free to override it to get a custom deadzone directly or via the helper setCenteredDeadzone.
		/// </summary>
		public RectangleF deadzone;

		/// <summary>
		/// offset from the screen center that the camera will focus on
		/// </summary>
		public Vector2 focusOffset;

		/// <summary>
		/// If true, the camera position will not got out of the map rectangle (0,0, mapwidth, mapheight)
		/// </summary>
		public bool mapLockEnabled;

		/// <summary>
		/// Contains the width and height of the current map.
		/// </summary>
		public Vector2 mapSize;

		Entity _targetEntity;
		Collider _targetCollider;
		Vector2 _desiredPositionDelta;
		CameraStyle _cameraStyle;
		RectangleF _worldSpaceDeadzone;

		
		public FollowCamera( Entity targetEntity, Camera camera, CameraStyle cameraStyle = CameraStyle.LockOn  )
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			this.camera = camera;
		}


		public FollowCamera( Entity targetEntity, CameraStyle cameraStyle = CameraStyle.LockOn ) : this( targetEntity, null, cameraStyle )
		{}


		public override void onAddedToEntity()
		{
			if( camera == null )
				camera = entity.scene.camera;

			follow( _targetEntity, _cameraStyle );

			// listen for changes in screen size so we can keep our deadzone properly positioned
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		public override void onRemovedFromEntity()
		{
			Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		void IUpdatable.update()
		{
			// translate the deadzone to be in world space
			var halfScreen = camera.bounds.size * 0.5f;
			_worldSpaceDeadzone.x = camera.position.X - halfScreen.X + deadzone.x + focusOffset.X;
			_worldSpaceDeadzone.y = camera.position.Y - halfScreen.Y + deadzone.y + focusOffset.Y;
			_worldSpaceDeadzone.width = deadzone.width;
			_worldSpaceDeadzone.height = deadzone.height;

			if( _targetEntity != null )
				updateFollow();

			camera.position = Vector2.Lerp( camera.position, camera.position + _desiredPositionDelta, followLerp );
			camera.entity.transform.roundPosition();

			if( mapLockEnabled )
			{
				camera.position = clampToMapSize( camera.position );
				camera.entity.transform.roundPosition();   
			}
		}


		/// <summary>
		/// Clamps the camera so it never leaves the visible area of the map.
		/// </summary>
		/// <returns>The to map size.</returns>
		/// <param name="position">Position.</param>
		Vector2 clampToMapSize( Vector2 position )
		{
			var halfScreen = new Vector2( camera.bounds.width, camera.bounds.height ) * 0.5f;
			var cameraMax = new Vector2( mapSize.X - halfScreen.X, mapSize.Y - halfScreen.Y );

			return Vector2.Clamp( position, halfScreen, cameraMax );
		}


        public override void debugRender( Graphics graphics )
		{
			if( _cameraStyle == CameraStyle.LockOn )
				graphics.batcher.drawHollowRect( _worldSpaceDeadzone.x - 5, _worldSpaceDeadzone.y - 5, _worldSpaceDeadzone.width, _worldSpaceDeadzone.height, Color.DarkRed );
			else
				graphics.batcher.drawHollowRect( _worldSpaceDeadzone, Color.DarkRed );
		}


		void onGraphicsDeviceReset()
		{
			// we need this to occur on the next frame so the camera bounds are updated
			Core.schedule( 0f, this, t =>
			{
				var self = t.context as FollowCamera;
				self.follow( self._targetEntity, self._cameraStyle );
			} );
		}


		void updateFollow()
		{
			_desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

			if( _cameraStyle == CameraStyle.LockOn )
			{
				var targetX = _targetEntity.transform.position.X;
				var targetY = _targetEntity.transform.position.Y;

				// x-axis
				if( _worldSpaceDeadzone.x > targetX )
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;
				else if( _worldSpaceDeadzone.x < targetX )
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;

				// y-axis
				if( _worldSpaceDeadzone.y < targetY )
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;
				else if( _worldSpaceDeadzone.y > targetY )
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;
			}
			else
			{
				// make sure we have a targetCollider for CameraWindow. If we dont bail out.
				if( _targetCollider == null )
				{
					_targetCollider = _targetEntity.getComponent<Collider>();
					if( _targetCollider == null )
						return;
				}
				
				var targetBounds = _targetEntity.getComponent<Collider>().bounds;
				if( !_worldSpaceDeadzone.contains( targetBounds ) )
				{
					// x-axis
					if( _worldSpaceDeadzone.left > targetBounds.left )
						_desiredPositionDelta.X = targetBounds.left - _worldSpaceDeadzone.left;
					else if( _worldSpaceDeadzone.right < targetBounds.right )
						_desiredPositionDelta.X = targetBounds.right - _worldSpaceDeadzone.right;

					// y-axis
					if( _worldSpaceDeadzone.bottom < targetBounds.bottom )
						_desiredPositionDelta.Y = targetBounds.bottom - _worldSpaceDeadzone.bottom;
					else if( _worldSpaceDeadzone.top > targetBounds.top )
						_desiredPositionDelta.Y = targetBounds.top - _worldSpaceDeadzone.top;
				}
			}
		}


		public void follow( Entity targetEntity, CameraStyle cameraStyle = CameraStyle.CameraWindow )
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			var cameraBounds = camera.bounds;

			switch( _cameraStyle )
			{
				case CameraStyle.CameraWindow:
					var w = ( cameraBounds.width / 6 );
					var h = ( cameraBounds.height / 3 );
					deadzone = new RectangleF( ( cameraBounds.width - w ) / 2, ( cameraBounds.height - h ) / 2, w, h );
					break;
				case CameraStyle.LockOn:
					deadzone = new RectangleF( cameraBounds.width / 2, cameraBounds.height / 2, 10, 10 );
					break;
			}
		}


		/// <summary>
		/// sets up the deadzone centered in the current cameras bounds with the given size
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void setCenteredDeadzone( int width, int height )
		{
			Assert.isFalse( camera == null, "camera is null. We cant get its bounds if its null. Either set it or wait until after this Component is added to the Entity." );
			var cameraBounds = camera.bounds;
			deadzone = new RectangleF( ( cameraBounds.width - width ) / 2, ( cameraBounds.height - height ) / 2, width, height );
		}

	}
}

