using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// basic follow camera. LockOn mode uses no deadzone and just centers the camera on the target. CameraWindow mode wraps a deadzone
	/// around the target allowing it to move within the deadzone without moving the camera.
	/// </summary>
	public class FollowCamera : Component
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
		public Rectangle deadzone;

		/// <summary>
		/// offset from the screen center that the camera will focus on
		/// </summary>
		public Vector2 focusOffset;

		Entity _targetEntity;
		Vector2 _desiredPositionDelta;
		CameraStyle _cameraStyle;
		Rectangle _worldSpaceDeadzone;

		
		public FollowCamera( Entity targetEntity, Camera camera )
		{
			_targetEntity = targetEntity;
			this.camera = camera;
		}


		public FollowCamera( Entity targetEntity ) : this( targetEntity, null )
		{}


		public override void onAwake()
		{
			if( camera == null )
				camera = entity.scene.camera;

			follow( _targetEntity );

			// listen for changes in screen size so we can keep our deadzone properly positioned
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		public override void onRemovedFromEntity()
		{
			Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		public override void update()
		{
			// translate the deadzone to be in world space
			_worldSpaceDeadzone.X = (int)( camera.position.X - camera.origin.X + deadzone.X + focusOffset.X );
			_worldSpaceDeadzone.Y = (int)( camera.position.Y - camera.origin.Y + deadzone.Y + focusOffset.Y );
			_worldSpaceDeadzone.Width = deadzone.Width;
			_worldSpaceDeadzone.Height = deadzone.Height;

			if( _targetEntity != null )
				updateFollow();

			camera.position = Vector2.Lerp( camera.position, camera.position + _desiredPositionDelta, followLerp );
		}


		public override void debugRender( Graphics graphics )
		{
			if( _cameraStyle == CameraStyle.LockOn )
				graphics.spriteBatch.drawHollowRect( _worldSpaceDeadzone.X - 5, _worldSpaceDeadzone.Y - 5, _worldSpaceDeadzone.Width, _worldSpaceDeadzone.Height, Color.DarkRed );
			else
				graphics.spriteBatch.drawHollowRect( _worldSpaceDeadzone, Color.DarkRed );
		}


		void onGraphicsDeviceReset()
		{
			follow( _targetEntity, _cameraStyle );
		}


		void updateFollow()
		{
			_desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

			if( _cameraStyle == CameraStyle.LockOn )
			{
				var targetX = _targetEntity.position.X;
				var targetY = _targetEntity.position.Y;

				// x-axis
				if( _worldSpaceDeadzone.X > targetX )
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.X;
				else if( _worldSpaceDeadzone.X < targetX )
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.X;

				// y-axis
				if( _worldSpaceDeadzone.Y < targetY )
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.Y;
				else if( _worldSpaceDeadzone.Y > targetY )
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.Y;
			}
			else
			{
				var targetBounds = _targetEntity.collider.bounds;
				if( !_worldSpaceDeadzone.Contains( targetBounds ) )
				{
					// x-axis
					if( _worldSpaceDeadzone.Left > targetBounds.Left )
						_desiredPositionDelta.X = targetBounds.Left - _worldSpaceDeadzone.Left;
					else if( _worldSpaceDeadzone.Right < targetBounds.Right )
						_desiredPositionDelta.X = targetBounds.Right - _worldSpaceDeadzone.Right;

					// y-axis
					if( _worldSpaceDeadzone.Bottom < targetBounds.Bottom )
						_desiredPositionDelta.Y = targetBounds.Bottom - _worldSpaceDeadzone.Bottom;
					else if( _worldSpaceDeadzone.Top > targetBounds.Top )
						_desiredPositionDelta.Y = targetBounds.Top - _worldSpaceDeadzone.Top;
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
					var w = ( cameraBounds.Width / 6 );
					var h = ( cameraBounds.Height / 3 );
					deadzone = new Rectangle( ( cameraBounds.Width - w ) / 2, ( cameraBounds.Height - h ) / 2, w, h );
					break;
				case CameraStyle.LockOn:
					deadzone = new Rectangle( cameraBounds.Width / 2, cameraBounds.Height / 2, 10, 10 );
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
			var cameraBounds = camera.bounds;
			deadzone = new Rectangle( ( cameraBounds.Width - width ) / 2, ( cameraBounds.Height - height ) / 2, width, height );
		}

	}
}

