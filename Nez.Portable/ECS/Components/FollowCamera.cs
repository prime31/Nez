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

		public enum Measurement
		{
			/// <summary>
			/// Size is measured in pixel.
			/// Does not change with the <see cref="Camera.Zoom"/> level.
			/// </summary>
			FixedPixel,
			/// <summary>
			/// Size is measured in % of Camera Screensize.
			/// Where 1.0f equals the whole Screen and 0.5f is half the screen size.
			/// Scales automatically with <see cref="Camera.Zoom"/> level.
			/// </summary>
			ScaledScreenSpace
		}

		public Camera Camera;

		/// <summary>
		/// how fast the camera closes the distance to the target position
		/// </summary>
		public float FollowLerp = 0.1f;

		/// <summary>
		/// when in <see cref="CameraStyle.CameraWindow"/> mode used as a bounding box around the camera position to allow movement within it without moving the camera.
		/// when in <see cref="CameraStyle.LockOn"/> mode only the deadzone x/y values are used as offset.
		/// This is set to sensible defaults when you call <see cref="Follow"/> but you are
		/// free to override <see cref="Deadzone"/> to get a custom deadzone directly or via the helper <see cref="SetCenteredDeadzone"/>.
		/// </summary>
		public RectangleF Deadzone;

		private Measurement _deadzoneMeasurement = Measurement.ScaledScreenSpace;

		/// <summary>
		/// offset from the screen center that the camera will focus on
		/// </summary>
		public Vector2 FocusOffset;

		/// <summary>
		/// If true, the camera position will not got out of the map rectangle (0,0, mapwidth, mapheight)
		/// </summary>
		public bool MapLockEnabled;

		/// <summary>
		/// Contains the width and height of the current map.
		/// </summary>
		public Vector2 MapSize;

		Entity _targetEntity;
		Collider _targetCollider;
		Vector2 _desiredPositionDelta;
		CameraStyle _cameraStyle;
		RectangleF _worldSpaceDeadzone;


		public FollowCamera(Entity targetEntity, Camera camera, CameraStyle cameraStyle = CameraStyle.LockOn, Measurement deadzoneMeasurement = Measurement.FixedPixel)
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			_deadzoneMeasurement = deadzoneMeasurement;
			Camera = camera;
		}

		public FollowCamera(Entity targetEntity, CameraStyle cameraStyle = CameraStyle.LockOn, Measurement deadzoneMeasurement = Measurement.FixedPixel) 
			: this(targetEntity, null, cameraStyle)
		{
		}

		public FollowCamera() : this(null, null)
		{
		}

		public override void OnAddedToEntity()
		{
			if (Camera == null)
				Camera = Entity.Scene.Camera;

			Follow(_targetEntity, _cameraStyle);

			// listen for changes in screen size so we can keep our deadzone properly positioned
			Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
		}

		public override void OnRemovedFromEntity()
		{
			Core.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
		}

		public virtual void Update()
		{
			// calculate the current deadzone around the camera
			if (_deadzoneMeasurement == Measurement.FixedPixel)
			{
				_worldSpaceDeadzone.X = Camera.Position.X + Deadzone.X + FocusOffset.X;
				_worldSpaceDeadzone.Y = Camera.Position.Y + Deadzone.Y + FocusOffset.Y;
				_worldSpaceDeadzone.Width = Deadzone.Width;
				_worldSpaceDeadzone.Height = Deadzone.Height;
			}
			else
			{
				var screen = Camera.Bounds.Size;
				_worldSpaceDeadzone.Width = screen.X * Deadzone.Width;
				_worldSpaceDeadzone.Height = screen.Y * Deadzone.Height;

				_worldSpaceDeadzone.X = Camera.Position.X - (_worldSpaceDeadzone.Width / 2) + FocusOffset.X;
				_worldSpaceDeadzone.Y = Camera.Position.Y - (_worldSpaceDeadzone.Height / 2) + FocusOffset.Y;
			}

			if (_targetEntity != null)
				UpdateFollow();

			Camera.Position = Vector2.Lerp(Camera.Position, Camera.Position + _desiredPositionDelta, FollowLerp);
			Camera.Entity.Transform.RoundPosition();

			if (MapLockEnabled)
			{
				Camera.Position = ClampToMapSize(Camera.Position);
				Camera.Entity.Transform.RoundPosition();
			}
		}

		/// <summary>
		/// Clamps the camera so it never leaves the visible area of the map.
		/// </summary>
		/// <returns>The to map size.</returns>
		/// <param name="position">Position.</param>
		Vector2 ClampToMapSize(Vector2 position)
		{
			var halfScreen = new Vector2(Camera.Bounds.Width, Camera.Bounds.Height) * 0.5f;
			var cameraMax = new Vector2(MapSize.X - halfScreen.X, MapSize.Y - halfScreen.Y);

			return Vector2.Clamp(position, halfScreen, cameraMax);
		}

		public override void DebugRender(Batcher batcher)
		{
			if (_cameraStyle == CameraStyle.LockOn)
				batcher.DrawHollowRect(_worldSpaceDeadzone.X - 5, _worldSpaceDeadzone.Y - 5, 10, 10, Color.DarkRed);
			else
				batcher.DrawHollowRect(_worldSpaceDeadzone, Color.DarkRed);
		}

		void OnGraphicsDeviceReset()
		{
			// we need this to occur on the next frame so the camera bounds are updated
			Core.Schedule(0f, this, t =>
			{
				var self = t.Context as FollowCamera;
				self.Follow(self._targetEntity, self._cameraStyle, self._deadzoneMeasurement);
			});
		}

		void UpdateFollow()
		{
			_desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

			if (_cameraStyle == CameraStyle.LockOn)
			{
				var targetX = _targetEntity.Transform.Position.X;
				var targetY = _targetEntity.Transform.Position.Y;

				// x-axis
				if (_worldSpaceDeadzone.X > targetX)
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.X;
				else if (_worldSpaceDeadzone.X < targetX)
					_desiredPositionDelta.X = targetX - _worldSpaceDeadzone.X;

				// y-axis
				if (_worldSpaceDeadzone.Y < targetY)
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.Y;
				else if (_worldSpaceDeadzone.Y > targetY)
					_desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.Y;
			}
			else
			{
				// make sure we have a targetCollider for CameraWindow. If we dont bail out.
				if (_targetCollider == null)
				{
					_targetCollider = _targetEntity.GetComponent<Collider>();
					if (_targetCollider == null)
						return;
				}

				var targetBounds = _targetEntity.GetComponent<Collider>().Bounds;
				if (!_worldSpaceDeadzone.Contains(targetBounds))
				{
					// x-axis
					if (_worldSpaceDeadzone.Left > targetBounds.Left)
						_desiredPositionDelta.X = targetBounds.Left - _worldSpaceDeadzone.Left;
					else if (_worldSpaceDeadzone.Right < targetBounds.Right)
						_desiredPositionDelta.X = targetBounds.Right - _worldSpaceDeadzone.Right;

					// y-axis
					if (_worldSpaceDeadzone.Bottom < targetBounds.Bottom)
						_desiredPositionDelta.Y = targetBounds.Bottom - _worldSpaceDeadzone.Bottom;
					else if (_worldSpaceDeadzone.Top > targetBounds.Top)
						_desiredPositionDelta.Y = targetBounds.Top - _worldSpaceDeadzone.Top;
				}
			}
		}

		public void Follow(Entity targetEntity, CameraStyle cameraStyle = CameraStyle.CameraWindow, Measurement deadzoneMeasurement = Measurement.ScaledScreenSpace)
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			_deadzoneMeasurement = deadzoneMeasurement;
			
			var cameraBounds = Camera.Bounds;

			switch (_cameraStyle)
			{
				case CameraStyle.CameraWindow:
					if (_deadzoneMeasurement == Measurement.ScaledScreenSpace)
					{
						var w = (1.0f / 6.0f);
						var h = (1.0f / 3.0f);
						Deadzone = new RectangleF(-w / 2, -h / 2, w, h);
					}
					else
					{
						var w = (cameraBounds.Width / 6);
						var h = (cameraBounds.Height / 3);
						Deadzone = new RectangleF(-w / 2, -h / 2, w, h);
					}
					break;
				case CameraStyle.LockOn:
					if (_deadzoneMeasurement == Measurement.ScaledScreenSpace)
					{
						Deadzone = RectangleF.Empty;
					}
					else
					{
						Deadzone = new RectangleF(-5, -5, 10, 10);	
					}
					break;
			}
		}

		/// <summary>
		/// sets up the deadzone centered in the current cameras bounds with the given size
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void SetCenteredDeadzone(int width, int height)
		{
			Insist.IsFalse(Camera == null,
				"camera is null. We cant get its bounds if its null. Either set it or wait until after this Component is added to the Entity.");
			var cameraBounds = Camera.Bounds;
			
			Deadzone = new RectangleF(-width / 2, -height / 2, width, height);
			
			_deadzoneMeasurement = Measurement.FixedPixel;
		}
		
		/// <summary>
		/// sets up the deadzone centered in the current cameras bounds with the given size
		/// </summary>
		/// <param name="width">Width in % of screenspace. Between 0.0 and 1.0</param>
		/// <param name="height">Height in % of screenspace. Between 0.0 and 1.0</param>
		public void SetCenteredDeadzoneInScreenspace(float width, float height)
		{
			Insist.IsFalse(Camera == null,
				"camera is null. We cant get its bounds if its null. Either set it or wait until after this Component is added to the Entity.");
			var cameraBounds = Camera.Bounds;
			Deadzone = new RectangleF(-width / 2, -height / 2, width, height);
			
			_deadzoneMeasurement = Measurement.ScaledScreenSpace;
		}
	}
}