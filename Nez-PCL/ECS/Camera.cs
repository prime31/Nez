using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	// TODO: all matrix creation should be done via Create method that takes a ref Matrix
	public class Camera
	{
		#region Fields and Properties

		public Vector2 position;
		public float rotation;
		public Vector2 origin;
		readonly GraphicsDevice _graphicsDevice;

		float _zoom;
		public float zoom
		{
			get { return _zoom; }
			set
			{
				_zoom = MathHelper.Clamp( value, _minimumZoom, _maximumZoom );
				_matrixesAreDirty = true;
			}
		}

		float _minimumZoom = 0.1f;
		public float minimumZoom
		{
			get { return _minimumZoom; }
			set
			{
				if( value < 0 )
					throw new ArgumentException( "MinimumZoom must be greater than zero" );

				if( zoom < value )
					zoom = minimumZoom;

				_minimumZoom = value;
			}
		}

		float _maximumZoom = 5f;
		public float maximumZoom
		{
			get { return _maximumZoom; }
			set
			{
				if( value < 0 )
					throw new ArgumentException( "MaximumZoom must be greater than zero" );

				if( zoom > value )
					zoom = value;

				_maximumZoom = value;
			}
		}

		Rectangle _bounds;
		/// <summary>
		/// world-space bounds of the camera. useful for culling.
		/// </summary>
		/// <value>The bounds.</value>
		public Rectangle bounds
		{
			get
			{
				if( _matrixesAreDirty )
					updateMatrixes();

				if( _boundsAreDirty )
				{
					var topLeft = screenToWorldPoint( new Vector2( _graphicsDevice.Viewport.X, _graphicsDevice.Viewport.Y ) );
					var bottomRight = screenToWorldPoint( new Vector2( _graphicsDevice.Viewport.X + _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Y + _graphicsDevice.Viewport.Height ) );

					_bounds.Location = topLeft.ToPoint();
					_bounds.Width = (int)( bottomRight.X - topLeft.X );
					_bounds.Height = (int)( bottomRight.Y - topLeft.Y );

					_boundsAreDirty = false;
				}

				return _bounds;
			}
		}

		Matrix _transformMatrix = Matrix.Identity;
		public Matrix transformMatrix
		{
			get
			{
				if( _matrixesAreDirty )
					updateMatrixes();
				return _transformMatrix;
			}
		}

		Matrix _inverseTransformMatrix = Matrix.Identity;
		public Matrix inverseTransformMatrix
		{
			get
			{
				if( _matrixesAreDirty )
					updateMatrixes();
				return _inverseTransformMatrix;
			}
		}

		ViewportAdapter _viewportAdapter;
		/// <summary>
		/// used for automatic scaling/boxing of the viewport and translation to/from world/screen positions
		/// </summary>
		public ViewportAdapter viewportAdapter
		{
			get { return _viewportAdapter; }
			set
			{
				if( _viewportAdapter != value )
				{
					_viewportAdapter = value;
					_matrixesAreDirty = true;
					_boundsAreDirty = true;
				}
			}
		}

		float _near = -10f;
		float _far = 10f;
		bool _matrixesAreDirty = true;
		bool _boundsAreDirty = true;

		#endregion


		public Camera( GraphicsDevice graphicsDevice, float near = 0, float far = 1 )
		{
			_graphicsDevice = graphicsDevice;

			rotation = 0;
			zoom = 1;
			_near = near;
			_far = far;

			// listen for screen resizes and graphics resets so we can dirty our bounds when it happens
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		void onGraphicsDeviceReset()
		{
			_boundsAreDirty = true;

			// if we have a viewport adapter we also dirty the matrixes since it will be modifying itself
			if( viewportAdapter != null )
				_matrixesAreDirty = true;
		}


		void updateMatrixes()
		{
			_transformMatrix =
				Matrix.CreateTranslation( -(int)position.X, -(int)position.Y, 0f ) * // position
				Matrix.CreateScale( zoom, zoom, 1f ) * // scale ->
				Matrix.CreateRotationZ( rotation ) * // rotation
				Matrix.CreateTranslation( (int)origin.X, (int)origin.Y, 0f ); // translate -origin

			// if we have a ViewportAdapter take it into account
			if( _viewportAdapter != null )
				Matrix.Multiply( ref _transformMatrix, ref _viewportAdapter.scaleMatrix, out _transformMatrix );

			// calculate our inverse as well
			Matrix.Invert( ref _transformMatrix, out _inverseTransformMatrix );

			// whenever the Matrixes change the bounds are then invalid
			_boundsAreDirty = true;
			_matrixesAreDirty = false;
		}


		/// <summary>
		/// this forces the matrix and bounds dirty
		/// </summary>
		public void forceMatrixUpdate()
		{
			_matrixesAreDirty = _boundsAreDirty = true;
		}


		public void unload()
		{
			Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
			if( viewportAdapter != null )
			{
				viewportAdapter.unload();
				viewportAdapter = null;
			}
		}


		public void roundPosition()
		{
			position.round();
			_matrixesAreDirty = true;
		}


		public void centerOrigin()
		{
			if( viewportAdapter != null )
				origin = new Vector2( viewportAdapter.virtualWidth / 2f, viewportAdapter.virtualHeight / 2f );
			else
				origin = new Vector2( _graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f );
			
			_matrixesAreDirty = true;
		}


		public void move( Vector2 direction )
		{
			position += Vector2.Transform( direction, Matrix.CreateRotationZ( -rotation ) );
			_matrixesAreDirty = true;
		}


		public void rotate( float deltaRadians )
		{
			rotation += deltaRadians;
			_matrixesAreDirty = true;
		}


		public void zoomIn( float deltaZoom )
		{
			zoom += deltaZoom;
		}


		public void zoomOut( float deltaZoom )
		{
			zoom -= deltaZoom;
		}


		/// <summary>
		/// converts a point from world coordinates to screen
		/// </summary>
		/// <returns>The to screen point.</returns>
		/// <param name="worldPosition">World position.</param>
		public Vector2 worldToScreenPoint( Vector2 worldPosition )
		{
			var pos = Vector2.Transform( worldPosition, transformMatrix );

			if( _viewportAdapter != null )
				pos = _viewportAdapter.screenToVirtualViewport( pos );
			return pos;
		}


		/// <summary>
		/// converts a oint from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 screenToWorldPoint( Vector2 screenPosition )
		{
			if( _viewportAdapter != null )
				screenPosition = _viewportAdapter.pointToVirtualViewport( screenPosition );
			return Vector2.Transform( screenPosition, inverseTransformMatrix );
		}


		/// <summary>
		/// converts a oint from screen coordinates to world
		/// </summary>
		/// <returns>The to world point.</returns>
		/// <param name="screenPosition">Screen position.</param>
		public Vector2 screenToWorldPoint( Point screenPosition ) => screenToWorldPoint( screenPosition.ToVector2() );


		/// <summary>
		/// gets this cameras project matrix
		/// </summary>
		/// <returns>The projection matrix.</returns>
		public Matrix getProjectionMatrix()
		{
			// not currently blocked with a dirty flag due to the core engine not using this
			return Matrix.CreateOrthographicOffCenter( 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, _near, _far );
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

	}
}

