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

		public Rectangle bounds
		{
			get
			{
				if( _matrixesAreDirty )
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

		float _near = -10f;
		float _far = 10f;
		bool _matrixesAreDirty = true;

		#endregion


		public Camera2D( GraphicsDevice graphicsDevice, float near = -10, float far = 10 )
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

			Matrix.Invert( ref _inverseTransformMatrix, out _inverseTransformMatrix );

			_matrixesAreDirty = false;
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
			return Vector2.Transform( worldPosition, transformMatrix );
		}


		public Vector2 screenToWorldPoint( Vector2 screenPosition )
		{
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

