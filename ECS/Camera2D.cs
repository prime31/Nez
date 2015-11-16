using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	// TODO: all matrix creation should be done via Create method that takes a ref Matrix
	public class Camera2D
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

		/// <summary>
		/// world-space bounds of the camera. useful for culling.
		/// </summary>
		/// <value>The bounds.</value>
		public Rectangle bounds
		{
			get
			{
				if( _matrixesAreDirty || ( _viewportAdapter != null && _viewportAdapter.hasDirtyMatrix ) )
					updateMatrixes();

				var topLeft = screenToWorldPoint( Vector2.Zero );
				// TODO: virtual viewports will require this to change
				var bottomRight = screenToWorldPoint( new Vector2( _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height ) );

				return new Rectangle( (int)topLeft.X, (int)topLeft.Y, (int)( bottomRight.X - topLeft.X ), (int)( bottomRight.Y - topLeft.Y ) );
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

		public ViewportAdapter _viewportAdapter;
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
				}
			}
		}

		float _near = -10f;
		float _far = 10f;
		bool _matrixesAreDirty = true;

		#endregion


		public Camera2D( GraphicsDevice graphicsDevice, float near = 0, float far = 1 )
		{
			_graphicsDevice = graphicsDevice;

			rotation = 0;
			zoom = 1;
			_near = near;
			_far = far;
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
				_transformMatrix *= _viewportAdapter.scaleMatrix;


			Matrix.Invert( ref _transformMatrix, out _inverseTransformMatrix );

			_matrixesAreDirty = false;

			if( _viewportAdapter != null )
				_viewportAdapter.hasDirtyMatrix = false;
		}


		public void roundPosition()
		{
			position.round();
			_matrixesAreDirty = true;
		}


		public void centerOrigin()
		{
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


		public Vector2 worldToScreenPoint( Vector2 worldPosition )
		{
			var pos = Vector2.Transform( worldPosition, transformMatrix );

			if( _viewportAdapter != null )
				pos = _viewportAdapter.screenToVirtualViewport( pos );
			return pos;
		}


		public Vector2 screenToWorldPoint( Vector2 screenPosition )
		{
			if( _viewportAdapter != null )
				screenPosition = _viewportAdapter.pointToVirtualViewport( screenPosition );
			return Vector2.Transform( screenPosition, inverseTransformMatrix );
		}


		public Vector2 screenToWorldPoint( Point screenPosition ) => screenToWorldPoint( screenPosition.ToVector2() );


		public Matrix getProjectionMatrix()
		{
			// TODO: block with dirty flag. needs to be made dirty when Viewport size changes
			return Matrix.CreateOrthographicOffCenter( 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, _near, _far );
		}


		public Matrix getViewProjectionMatrix()
		{
			// TODO: block with dirty flag and use Matrix.Multiply to cache Matrixes
			return transformMatrix * getProjectionMatrix();
		}

	}
}

