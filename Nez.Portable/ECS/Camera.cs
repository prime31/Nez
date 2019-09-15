using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;


namespace Nez
{
	public class Camera : Component
	{
		struct CameraInset
		{
			internal float left;
			internal float right;
			internal float top;
			internal float bottom;
		}


		#region Fields and Properties

		#region 3D Camera Fields

		/// <summary>
		/// z-position of the 3D camera projections. Affects the fov greatly. Lower values make the objects appear very long in the z-direction.
		/// </summary>
		public float PositionZ3D = 2000f;

		/// <summary>
		/// near clip plane of the 3D camera projection
		/// </summary>
		public float NearClipPlane3D = 0.0001f;

		/// <summary>
		/// far clip plane of the 3D camera projection
		/// </summary>
		public float FarClipPlane3D = 5000f;

		#endregion


		/// <summary>
		/// shortcut to entity.transform.position
		/// </summary>
		/// <value>The position.</value>
		public Vector2 Position
		{
			get => Entity.Transform.Position;
			set => Entity.Transform.Position = value;
		}

		/// <summary>
		/// shortcut to entity.transform.rotation
		/// </summary>
		/// <value>The rotation.</value>
		public float Rotation
		{
			get => Entity.Transform.Rotation;
			set => Entity.Transform.Rotation = value;
		}

		/// <summary>
		/// raw zoom value. This is the exact value used for the scale matrix. Default is 1.
		/// </summary>
		/// <value>The raw zoom.</value>
		[Range(0, 30)]
		public float RawZoom
		{
			get => _zoom;
			set
			{
				if (value != _zoom)
				{
					_zoom = value;
					_areMatrixesDirty = true;
				}
			}
		}

		/// <summary>
		/// the zoom value should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom. This lets you set
		/// appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
		/// </summary>
		/// <value>The zoom.</value>
		[Range(-1, 1)]
		public float Zoom
		{
			get
			{
				if (_zoom == 0)
					return 1f;

				if (_zoom < 1)
					return Mathf.Map(_zoom, _minimumZoom, 1, -1, 0);

				return Mathf.Map(_zoom, 1, _maximumZoom, 0, 1);
			}
			set => SetZoom(value);
		}

		/// <summary>
		/// minimum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 0.3
		/// </summary>
		/// <value>The minimum zoom.</value>
		[Range(0, 30)]
		public float MinimumZoom
		{
			get => _minimumZoom;
			set => SetMinimumZoom(value);
		}

		/// <summary>
		/// maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
		/// </summary>
		/// <value>The maximum zoom.</value>
		[Range(0, 30)]
		public float MaximumZoom
		{
			get => _maximumZoom;
			set => SetMaximumZoom(value);
		}

		/// <summary>
		/// world-space bounds of the camera. useful for culling.
		/// </summary>
		/// <value>The bounds.</value>
		public RectangleF Bounds
		{
			get
			{
				if (_areMatrixesDirty)
					UpdateMatrixes();

				if (_areBoundsDirty)
				{
					// top-left and bottom-right are needed by either rotated or non-rotated bounds
					var topLeft = ScreenToWorldPoint(new Vector2(Core.GraphicsDevice.Viewport.X + _inset.left,
						Core.GraphicsDevice.Viewport.Y + _inset.top));
					var bottomRight = ScreenToWorldPoint(new Vector2(
						Core.GraphicsDevice.Viewport.X + Core.GraphicsDevice.Viewport.Width - _inset.right,
						Core.GraphicsDevice.Viewport.Y + Core.GraphicsDevice.Viewport.Height - _inset.bottom));

					if (Entity.Transform.Rotation != 0)
					{
						// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
						var topRight = ScreenToWorldPoint(new Vector2(
							Core.GraphicsDevice.Viewport.X + Core.GraphicsDevice.Viewport.Width - _inset.right,
							Core.GraphicsDevice.Viewport.Y + _inset.top));
						var bottomLeft = ScreenToWorldPoint(new Vector2(Core.GraphicsDevice.Viewport.X + _inset.left,
							Core.GraphicsDevice.Viewport.Y + Core.GraphicsDevice.Viewport.Height - _inset.bottom));

						var minX = Mathf.MinOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
						var maxX = Mathf.MaxOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
						var minY = Mathf.MinOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);
						var maxY = Mathf.MaxOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);

						_bounds.Location = new Vector2(minX, minY);
						_bounds.Width = maxX - minX;
						_bounds.Height = maxY - minY;
					}
					else
					{
						_bounds.Location = topLeft;
						_bounds.Width = bottomRight.X - topLeft.X;
						_bounds.Height = bottomRight.Y - topLeft.Y;
					}

					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// used to convert from world coordinates to screen
		/// </summary>
		/// <value>The transform matrix.</value>
		public Matrix2D TransformMatrix
		{
			get
			{
				if (_areMatrixesDirty)
					UpdateMatrixes();
				return _transformMatrix;
			}
		}

		/// <summary>
		/// used to convert from screen coordinates to world
		/// </summary>
		/// <value>The inverse transform matrix.</value>
		public Matrix2D InverseTransformMatrix
		{
			get
			{
				if (_areMatrixesDirty)
					UpdateMatrixes();
				return _inverseTransformMatrix;
			}
		}

		/// <summary>
		/// the 2D Cameras projection matrix
		/// </summary>
		/// <value>The projection matrix.</value>
		public Matrix ProjectionMatrix
		{
			get
			{
				if (_isProjectionMatrixDirty)
				{
					Matrix.CreateOrthographicOffCenter(0, Core.GraphicsDevice.Viewport.Width,
						Core.GraphicsDevice.Viewport.Height, 0, 0, -1, out _projectionMatrix);
					_isProjectionMatrixDirty = false;
				}

				return _projectionMatrix;
			}
		}

		/// <summary>
		/// gets the view-projection matrix which is the transformMatrix * the projection matrix
		/// </summary>
		/// <value>The view projection matrix.</value>
		public Matrix ViewProjectionMatrix => TransformMatrix * ProjectionMatrix;

		#region 3D Camera Matrixes

		/// <summary>
		/// returns a perspective projection for this camera for use when rendering 3D objects
		/// </summary>
		/// <value>The projection matrix3 d.</value>
		public Matrix ProjectionMatrix3D
		{
			get
			{
				var targetHeight = (Core.GraphicsDevice.Viewport.Height / _zoom);
				var fov = (float) Math.Atan(targetHeight / (2f * PositionZ3D)) * 2f;
				return Matrix.CreatePerspectiveFieldOfView(fov, Core.GraphicsDevice.Viewport.AspectRatio,
					NearClipPlane3D, FarClipPlane3D);
			}
		}

		/// <summary>
		/// returns a view Matrix via CreateLookAt for this camera for use when rendering 3D objects
		/// </summary>
		/// <value>The view matrix3 d.</value>
		public Matrix ViewMatrix3D
		{
			get
			{
				// we need to always invert the y-values to match the way Batcher/SpriteBatch does things
				var position3D = new Vector3(Position.X, -Position.Y, PositionZ3D);
				return Matrix.CreateLookAt(position3D, position3D + Vector3.Forward, Vector3.Up);
			}
		}

		#endregion

		public Vector2 Origin
		{
			get => _origin;
			internal set
			{
				if (_origin != value)
				{
					_origin = value;
					_areMatrixesDirty = true;
				}
			}
		}


		float _zoom;
		float _minimumZoom = 0.3f;
		float _maximumZoom = 3f;
		RectangleF _bounds;
		CameraInset _inset;
		Matrix2D _transformMatrix = Matrix2D.Identity;
		Matrix2D _inverseTransformMatrix = Matrix2D.Identity;
		Matrix _projectionMatrix;
		Vector2 _origin;

		bool _areMatrixesDirty = true;
		bool _areBoundsDirty = true;
		bool _isProjectionMatrixDirty = true;

		#endregion


		public Camera()
		{
			SetZoom(0);
		}


		/// <summary>
		/// when the scene render target size changes we update the cameras origin and adjust the position to keep it where it was
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		internal void OnSceneRenderTargetSizeChanged(int newWidth, int newHeight)
		{
			_isProjectionMatrixDirty = true;
			var oldOrigin = _origin;
			Origin = new Vector2(newWidth / 2f, newHeight / 2f);

			// offset our position to match the new center
			Entity.Transform.Position += (_origin - oldOrigin);
		}


		protected virtual void UpdateMatrixes()
		{
			if (!_areMatrixesDirty)
				return;

			Matrix2D tempMat;
			_transformMatrix =
				Matrix2D.CreateTranslation(-Entity.Transform.Position.X, -Entity.Transform.Position.Y); // position

			if (_zoom != 1f)
			{
				Matrix2D.CreateScale(_zoom, _zoom, out tempMat); // scale ->
				Matrix2D.Multiply(ref _transformMatrix, ref tempMat, out _transformMatrix);
			}

			if (Entity.Transform.Rotation != 0f)
			{
				Matrix2D.CreateRotation(Entity.Transform.Rotation, out tempMat); // rotation
				Matrix2D.Multiply(ref _transformMatrix, ref tempMat, out _transformMatrix);
			}

			Matrix2D.CreateTranslation((int) _origin.X, (int) _origin.Y, out tempMat); // translate -origin
			Matrix2D.Multiply(ref _transformMatrix, ref tempMat, out _transformMatrix);

			// calculate our inverse as well
			Matrix2D.Invert(ref _transformMatrix, out _inverseTransformMatrix);

			// whenever the matrix changes the bounds are then invalid
			_areBoundsDirty = true;
			_areMatrixesDirty = false;
		}


		#region Fluent setters

		/// <summary>
		/// sets the amount used to inset the camera bounds from the viewport edge
		/// </summary>
		/// <param name="left">The amount to set the left bounds in from the viewport.</param>
		/// <param name="right">The amount to set the right bounds in from the viewport.</param>
		/// <param name="top">The amount to set the top bounds in from the viewport.</param>
		/// <param name="bottom">The amount to set the bottom bounds in from the viewport.</param>
		public Camera SetInset(float left, float right, float top, float bottom)
		{
			_inset = new CameraInset {left = left, right = right, top = top, bottom = bottom};
			_areBoundsDirty = true;
			return this;
		}


		/// <summary>
		/// shortcut to entity.transform.setPosition
		/// </summary>
		/// <param name="position">Position.</param>
		public Camera SetPosition(Vector2 position)
		{
			Entity.Transform.SetPosition(position);
			return this;
		}


		/// <summary>
		/// shortcut to entity.transform.setRotation
		/// </summary>
		/// <param name="radians">Radians.</param>
		public Camera SetRotation(float radians)
		{
			Entity.Transform.SetRotation(radians);
			return this;
		}


		/// <summary>
		/// shortcut to entity.transform.setRotationDegrees
		/// </summary>
		/// <param name="degrees">Degrees.</param>
		public Camera SetRotationDegrees(float degrees)
		{
			Entity.Transform.SetRotationDegrees(degrees);
			return this;
		}


		/// <summary>
		/// sets the the zoom value which should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom.
		/// This lets you set appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
		/// </summary>
		/// <param name="zoom">Zoom.</param>
		public Camera SetZoom(float zoom)
		{
			var newZoom = Mathf.Clamp(zoom, -1, 1);
			if (newZoom == 0)
				_zoom = 1f;
			else if (newZoom < 0)
				_zoom = Mathf.Map(newZoom, -1, 0, _minimumZoom, 1);
			else
				_zoom = Mathf.Map(newZoom, 0, 1, 1, _maximumZoom);

			_areMatrixesDirty = true;

			return this;
		}


		/// <summary>
		/// minimum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 0.3
		/// </summary>
		/// <param name="value">Value.</param>
		public Camera SetMinimumZoom(float minZoom)
		{
			Insist.IsTrue(minZoom > 0, "minimumZoom must be greater than zero");

			if (_zoom < minZoom)
				_zoom = MinimumZoom;

			_minimumZoom = minZoom;
			return this;
		}


		/// <summary>
		/// maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
		/// </summary>
		/// <param name="maxZoom">Max zoom.</param>
		public Camera SetMaximumZoom(float maxZoom)
		{
			Insist.IsTrue(maxZoom > 0, "MaximumZoom must be greater than zero");

			if (_zoom > maxZoom)
				_zoom = maxZoom;

			_maximumZoom = maxZoom;
			return this;
		}

		#endregion


		/// <summary>
		/// this forces the matrix and bounds dirty
		/// </summary>
		public void ForceMatrixUpdate()
		{
			// dirtying the matrix will automatically dirty the bounds as well
			_areMatrixesDirty = true;
		}


		#region component overrides

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			_areMatrixesDirty = true;
		}

		#endregion


		#region zoom helpers

		public void ZoomIn(float deltaZoom)
		{
			Zoom += deltaZoom;
		}


		public void ZoomOut(float deltaZoom)
		{
			Zoom -= deltaZoom;
		}

		#endregion


		#region transformations

		/// <summary>
		/// converts a point from world coordinates to screen
		/// </summary>
		/// <returns>The to screen point.</returns>
		/// <param name="worldPosition">World position.</param>
		public Vector2 WorldToScreenPoint(Vector2 worldPosition)
		{
			UpdateMatrixes();
			Vector2Ext.Transform(ref worldPosition, ref _transformMatrix, out worldPosition);
			return worldPosition;
		}


		/// <summary>
		/// converts a point from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
		{
			UpdateMatrixes();
			Vector2Ext.Transform(ref screenPosition, ref _inverseTransformMatrix, out screenPosition);
			return screenPosition;
		}


		/// <summary>
		/// converts a point from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 ScreenToWorldPoint(Point screenPosition)
		{
			return ScreenToWorldPoint(screenPosition.ToVector2());
		}


		/// <summary>
		/// returns the mouse position in world space
		/// </summary>
		/// <returns>The to world point.</returns>
		public Vector2 MouseToWorldPoint()
		{
			return ScreenToWorldPoint(Input.MousePosition);
		}


		/// <summary>
		/// returns the touch position in world space
		/// </summary>
		/// <returns>The to world point.</returns>
		public Vector2 TouchToWorldPoint(TouchLocation touch)
		{
			return ScreenToWorldPoint(touch.ScaledPosition());
		}

		#endregion
	}
}