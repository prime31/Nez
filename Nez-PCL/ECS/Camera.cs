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
		float _zoom;

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
			set
			{
				var newZoom = Mathf.clamp( value, -1, 1 );
				if( newZoom == 0 )
					_zoom = 1f;
				else if( newZoom < 0 )
					_zoom = Mathf.map( newZoom, -1, 0, _minimumZoom, 1 );
				else
					_zoom = Mathf.map( newZoom, 0, 1, 1, _maximumZoom );
				
				_areMatrixesDirty = true;
			}
		}

		/// <summary>
		/// minimum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 0.3
		/// </summary>
		/// <value>The minimum zoom.</value>
		public float minimumZoom
		{
			get { return _minimumZoom; }
			set
			{
				Assert.isTrue( value > 0, "minimumZoom must be greater than zero" );

				if( _zoom < value )
					_zoom = minimumZoom;

				_minimumZoom = value;
			}
		}
		float _minimumZoom = 0.3f;

		/// <summary>
		/// maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
		/// </summary>
		/// <value>The maximum zoom.</value>
		public float maximumZoom
		{
			get { return _maximumZoom; }
			set
			{
				Assert.isTrue( value > 0, "MaximumZoom must be greater than zero" );

				if( _zoom > value )
					_zoom = value;

				_maximumZoom = value;
			}
		}
		float _maximumZoom = 3f;

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
		RectangleF _bounds;

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
		Matrix _transformMatrix = Matrix.Identity;

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
		Matrix _inverseTransformMatrix = Matrix.Identity;

		protected Vector2 _origin;
		Vector2 origin
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


		float _near = -100f;
		float _far = 100f;
		bool _areMatrixesDirty = true;
		bool _areBoundsDirty = true;

		#endregion


		public Camera( float near = -100, float far = 100 )
		{
			zoom = 0;
			_near = near;
			_far = far;
		}


		/// <summary>
		/// when the scene render target size changes we update the cameras origin and adjust the position to keep it where it was
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		public void onSceneRenderTargetSizeChanged( int newWidth, int newHeight )
		{
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


		#region transforms and matrix

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


		/// <summary>
		/// gets this cameras project matrix
		/// </summary>
		/// <returns>The projection matrix.</returns>
		public Matrix getProjectionMatrix()
		{
			// not currently blocked with a dirty flag due to the core engine not using this
			return Matrix.CreateOrthographicOffCenter( 0, Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Height, 0, _near, _far );
		}


		/// <summary>
		/// gets the view-projection matrix which is the transformMatrix * the projection matrix
		/// </summary>
		/// <returns>The view projection matrix.</returns>
		public Matrix getViewProjectionMatrix()
		{
			// not currently blocked with a dirty flag due to the core engine not using this
			return transformMatrix * getProjectionMatrix();
		}

		#endregion

	}
}

