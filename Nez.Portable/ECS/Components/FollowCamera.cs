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

		public Camera Camera;

		/// <summary>
		/// how fast the camera closes the distance to the target position
		/// </summary>
		public float FollowLerp = 0.1f;

		/// <summary>
		/// when in CameraWindow mode the width/height is used as a bounding box to allow movement within it without moving the camera.
		/// when in LockOn mode only the deadzone x/y values are used. This is set to sensible defaults when you call follow but you are
		/// free to override it to get a custom deadzone directly or via the helper setCenteredDeadzone.
		/// </summary>
		public RectangleF Deadzone;

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


		public FollowCamera(Entity targetEntity, Camera camera, CameraStyle cameraStyle = CameraStyle.LockOn)
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			Camera = camera;
		}

		public FollowCamera(Entity targetEntity, CameraStyle cameraStyle = CameraStyle.LockOn) : this(targetEntity,
			null, cameraStyle)
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

		void IUpdatable.Update()
		{
			// translate the deadzone to be in world space
			var halfScreen = Camera.Bounds.Size * 0.5f;
			_worldSpaceDeadzone.X = Camera.Position.X - halfScreen.X + Deadzone.X + FocusOffset.X;
			_worldSpaceDeadzone.Y = Camera.Position.Y - halfScreen.Y + Deadzone.Y + FocusOffset.Y;
			_worldSpaceDeadzone.Width = Deadzone.Width;
			_worldSpaceDeadzone.Height = Deadzone.Height;

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
				batcher.DrawHollowRect(_worldSpaceDeadzone.X - 5, _worldSpaceDeadzone.Y - 5,
					_worldSpaceDeadzone.Width, _worldSpaceDeadzone.Height, Color.DarkRed);
			else
				batcher.DrawHollowRect(_worldSpaceDeadzone, Color.DarkRed);
		}

		void OnGraphicsDeviceReset()
		{
			// we need this to occur on the next frame so the camera bounds are updated
			Core.Schedule(0f, this, t =>
			{
				var self = t.Context as FollowCamera;
				self.Follow(self._targetEntity, self._cameraStyle);
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

		public void Follow(Entity targetEntity, CameraStyle cameraStyle = CameraStyle.CameraWindow)
		{
			_targetEntity = targetEntity;
			_cameraStyle = cameraStyle;
			var cameraBounds = Camera.Bounds;

			switch (_cameraStyle)
			{
				case CameraStyle.CameraWindow:
					var w = (cameraBounds.Width / 6);
					var h = (cameraBounds.Height / 3);
					Deadzone = new RectangleF((cameraBounds.Width - w) / 2, (cameraBounds.Height - h) / 2, w, h);
					break;
				case CameraStyle.LockOn:
					Deadzone = new RectangleF(cameraBounds.Width / 2, cameraBounds.Height / 2, 10, 10);
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
			Deadzone = new RectangleF((cameraBounds.Width - width) / 2, (cameraBounds.Height - height) / 2, width,
				height);
		}
	}
}