using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Camera : Component
	{
		#region Fields and Properties

		/// <summary>
		/// shortcut to entity.transform.position
		/// </summary>
		/// <value>The position.</value>
		public Vector2 position
		{
			get { return entity.transform.position; }
			set { entity.transform.position = value; }
		}

		/// <summary>
		/// shortcut to entity.transform.rotation
		/// </summary>
		/// <value>The rotation.</value>
		public float rotation
		{
			get { return entity.transform.rotation; }
			set { entity.transform.rotation = value; }
		}

		/// <summary>
		/// raw zoom value. This is the exact value used for the scale matrix. Default is 1.
		/// </summary>
		/// <value>The raw zoom.</value>
		public float rawZoom
		{
			get { return _zoom; }
			set
			{
				if( value != _zoom )
				{
					_zoom = value;
					_areBoundsDirty = true;
				}
			}
		}

		/// <summary>
		/// the zoom value should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom. This lets you set
		/// appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
		/// </summary>
		/// <value>The zoom.</value>
		public float zoom
		{
			get
			{
				if( _zoom == 0 )
					return 1f;
				else if( _zoom < 1 )
					return Mathf.map( _zoom, _minimumZoom, 1, -1, 0 );
				else
					return Mathf.map( _zoom, 1, _maximumZoom, 0, 1 );
			}
			set { setZoom( value ); }
		}

		/// <summary>
		/// minimum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 0.3
		/// </summary>
		/// <value>The minimum zoom.</value>
		public float minimumZoom
		{
			get { return _minimumZoom; }
			set { setMinimumZoom( value ); }
		}

		/// <summary>
		/// maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
		/// </summary>
		/// <value>The maximum zoom.</value>
		public float maximumZoom
		{
			get { return _maximumZoom; }
			set { setMaximumZoom( value ); }
		}

		/// <summary>
		/// world-space bounds of the camera. useful for culling.
		/// </summary>
		/// <value>The bounds.</value>
		public RectangleF bounds
		{
			get
			{
				if( _areMatrixesDirty )
					updateMatrixes();

				if( _areBoundsDirty )
				{
					// top-left and bottom-right are needed by either rotated or non-rotated bounds
					var topLeft = screenToWorldPoint( new Vector2( Core.graphicsDevice.Viewport.X, Core.graphicsDevice.Viewport.Y ) );
					var bottomRight = screenToWorldPoint( new Vector2( Core.graphicsDevice.Viewport.X + Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Y + Core.graphicsDevice.Viewport.Height ) );

					if( entity.transform.rotation != 0 )
					{
						// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
						var topRight = screenToWorldPoint( new Vector2( Core.graphicsDevice.Viewport.X + Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Y ) );
						var bottomLeft = screenToWorldPoint( new Vector2( Core.graphicsDevice.Viewport.X, Core.graphicsDevice.Viewport.Y + Core.graphicsDevice.Viewport.Height ) );	

						var minX = Mathf.minOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
						var maxX = Mathf.maxOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
						var minY = Mathf.minOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );
						var maxY = Mathf.maxOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );

						_bounds.location = new Vector2( minX, minY );
						_bounds.width = maxX - minX;
						_bounds.height = maxY - minY;
					}
					else
					{
						_bounds.location = topLeft;
						_bounds.width = bottomRight.X - topLeft.X;
						_bounds.height = bottomRight.Y - topLeft.Y;
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
		public Matrix transformMatrix
		{
			get
			{
				if( _areMatrixesDirty )
					updateMatrixes();
				return _transformMatrix;
			}
		}

		/// <summary>
		/// used to convert from screen coordinates to world
		/// </summary>
		/// <value>The inverse transform matrix.</value>
		public Matrix inverseTransformMatrix
		{
			get
			{
				if( _areMatrixesDirty )
					updateMatrixes();
				return _inverseTransformMatrix;
			}
		}

		/// <summary>
		/// the Cameras projection matrix
		/// </summary>
		/// <value>The projection matrix.</value>
		public Matrix projectionMatrix
		{
			get
			{
				if( _isProjectionMatrixDirty )
				{
					Matrix.CreateOrthographicOffCenter( 0, Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Height, 0, 0, -1, out _projectionMatrix );
					_isProjectionMatrixDirty = false;
				}
				return _projectionMatrix;
			}
		}

		/// <summary>
		/// gets the view-projection matrix which is the transformMatrix * the projection matrix
		/// </summary>
		/// <value>The view projection matrix.</value>
		public Matrix viewProjectionMatrix { get { return transformMatrix * projectionMatrix; } }

		internal Vector2 origin
		{
			get { return _origin; }
			set
			{
				if( _origin != value )
				{
					_origin = value;
					forceMatrixUpdate();
				}
			}
		}


		float _zoom;
		float _minimumZoom = 0.3f;
		float _maximumZoom = 3f;
		RectangleF _bounds;
		Matrix _transformMatrix = Matrix.Identity;
		Matrix _inverseTransformMatrix = Matrix.Identity;
		Matrix _projectionMatrix;
		Vector2 _origin;

		bool _areMatrixesDirty = true;
		bool _areBoundsDirty = true;
		bool _isProjectionMatrixDirty = true;

		#endregion


		public Camera()
		{
			setZoom( 0 );
		}


		/// <summary>
		/// when the scene render target size changes we update the cameras origin and adjust the position to keep it where it was
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		internal void onSceneRenderTargetSizeChanged( int newWidth, int newHeight )
		{
			_isProjectionMatrixDirty = true;
			var oldOrigin = _origin;
			origin = new Vector2( newWidth / 2f, newHeight / 2f );

			// offset our position to match the new center
			entity.transform.position += ( _origin - oldOrigin );
		}


		protected virtual void updateMatrixes()
		{
			if( !_areMatrixesDirty )
				return;

			Matrix tempMat;

			_transformMatrix = Matrix.CreateTranslation( -entity.transform.position.X, -entity.transform.position.Y, 0f ); // position

			if( _zoom != 1f )
			{
				Matrix.CreateScale( _zoom, _zoom, 1f, out tempMat ); // scale ->
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
			}

			if( entity.transform.rotation != 0f )
			{
				Matrix.CreateRotationZ( entity.transform.rotation, out tempMat ); // rotation
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
			}

			Matrix.CreateTranslation( (int)_origin.X, (int)_origin.Y, 0f, out tempMat ); // translate -origin
			Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );

			// calculate our inverse as well
			Matrix.Invert( ref _transformMatrix, out _inverseTransformMatrix );

			// whenever the matrix changes the bounds are then invalid
			_areBoundsDirty = true;
			_areMatrixesDirty = false;
		}


		#region Fluent setters

		/// <summary>
		/// shortcut to entity.transform.setPosition
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="value">Value.</param>
		public Camera setPosition( Vector2 position )
		{
			entity.transform.setPosition( position );
			return this;
		}


		/// <summary>
		/// shortcut to entity.transform.setRotation
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		public Camera setRotation( float radians )
		{
			entity.transform.setRotation( radians );
			return this;
		}


		/// <summary>
		/// shortcut to entity.transform.setRotationDegrees
		/// </summary>
		/// <returns>The rotation degrees.</returns>
		/// <param name="degrees">Degrees.</param>
		public Camera setRotationDegrees( float degrees )
		{
			entity.transform.setRotationDegrees( degrees );
			return this;
		}


		/// <summary>
		/// sets the the zoom value which should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom.
		/// This lets you set appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
		/// </summary>
		/// <returns>The zoom.</returns>
		/// <param name="zoom">Zoom.</param>
		public Camera setZoom( float zoom )
		{
			var newZoom = Mathf.clamp( zoom, -1, 1 );
			if( newZoom == 0 )
				_zoom = 1f;
			else if( newZoom < 0 )
				_zoom = Mathf.map( newZoom, -1, 0, _minimumZoom, 1 );
			else
				_zoom = Mathf.map( newZoom, 0, 1, 1, _maximumZoom );

			_areMatrixesDirty = true;

			return this;
		}


		/// <summary>
		/// minimum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 0.3
		/// </summary>
		/// <returns>The minimum zoom.</returns>
		/// <param name="value">Value.</param>
		public Camera setMinimumZoom( float minZoom )
		{
			Assert.isTrue( minZoom > 0, "minimumZoom must be greater than zero" );

			if( _zoom < minZoom )
				_zoom = minimumZoom;

			_minimumZoom = minZoom;
			return this;
		}


		/// <summary>
		/// maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
		/// </summary>
		/// <returns>The maximum zoom.</returns>
		/// <param name="maxZoom">Max zoom.</param>
		public Camera setMaximumZoom( float maxZoom )
		{
			Assert.isTrue( maxZoom > 0, "MaximumZoom must be greater than zero" );

			if( _zoom > maxZoom )
				_zoom = maxZoom;

			_maximumZoom = maxZoom;
			return this;
		}

		#endregion


		/// <summary>
		/// this forces the matrix and bounds dirty
		/// </summary>
		public void forceMatrixUpdate()
		{
			_areMatrixesDirty = _areBoundsDirty = true;
		}


		#region component overrides

		public override void onEntityTransformChanged()
		{
			forceMatrixUpdate();
		}

		#endregion


		#region zoom helpers

		public void zoomIn( float deltaZoom )
		{
			zoom += deltaZoom;
		}


		public void zoomOut( float deltaZoom )
		{
			zoom -= deltaZoom;
		}

		#endregion


		#region transformations

		/// <summary>
		/// converts a point from world coordinates to screen
		/// </summary>
		/// <returns>The to screen point.</returns>
		/// <param name="worldPosition">World position.</param>
		public Vector2 worldToScreenPoint( Vector2 worldPosition )
		{
			updateMatrixes();
			Vector2.Transform( ref worldPosition, ref _transformMatrix, out worldPosition );
			return worldPosition;
		}


		/// <summary>
		/// converts a point from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 screenToWorldPoint( Vector2 screenPosition )
		{
			updateMatrixes();
			Vector2.Transform( ref screenPosition, ref _inverseTransformMatrix, out screenPosition );
			return screenPosition;
		}


		/// <summary>
		/// converts a oint from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 screenToWorldPoint( Point screenPosition )
		{
			return screenToWorldPoint( screenPosition.ToVector2() );
		}

		#endregion

	}
}

