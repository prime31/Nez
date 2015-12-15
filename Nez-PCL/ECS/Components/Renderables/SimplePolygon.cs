using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class SimplePolygon : RenderableComponent
	{
		public override float width
		{
			get { return bounds.Width; }
		}

		public override float height
		{
			get { return bounds.Height; }
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleExt.boundsFromPolygonPoints( worldSpacePoints );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Polygon points converted to world space with transformations applied
		/// </summary>
		/// <value>The world space points.</value>
		public Vector2[] worldSpacePoints
		{
			get
			{
				if( _areBoundsDirty )
				{
					if( _rotation == 0f )
					{
						for( var i = 0; i < _points.Length; i++ )
							_worldSpacePoints[i] = _points[i] + entity.position + _localPosition - _origin;						
					}
					else
					{
						var matrix = transformMatrix;
						Vector2.Transform( _points, ref matrix, _worldSpacePoints );
					}

					// points changed so we need to update our verts
					var vertIndex = 0;
					for( var i = 1; i < _worldSpacePoints.Length - 1; i++ )
					{
						_verts[vertIndex].Position.X = _worldSpacePoints[0].X;
						_verts[vertIndex++].Position.Y = _worldSpacePoints[0].Y;
						_verts[vertIndex].Position.X = _worldSpacePoints[i].X;
						_verts[vertIndex++].Position.Y = _worldSpacePoints[i].Y;
						_verts[vertIndex].Position.X = _worldSpacePoints[i + 1].X;
						_verts[vertIndex++].Position.Y = _worldSpacePoints[i + 1].Y;
					}
				}
				return _worldSpacePoints;
			}
		}

		Matrix _transformMatrix;
		protected Matrix transformMatrix
		{
			get
			{
				var worldPosX = entity.position.X + _localPosition.X;
				var worldPosY = entity.position.Y + _localPosition.Y;
				var tempMat = Matrix.Identity;

				// set the reference point taking origin into account
				_transformMatrix = Matrix.CreateTranslation( -_origin.X, -_origin.Y, 0f ); // origin
				Matrix.CreateRotationZ( _rotation, out tempMat ); // rotation
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
				Matrix.CreateTranslation( _origin.X, _origin.Y, 0f, out tempMat ); // translate back from our origin
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
				Matrix.CreateTranslation( worldPosX, worldPosY, 0f, out tempMat ); // translate to our world space position
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );

				return _transformMatrix;
			}
		}

		Vector2[] _points;
		Vector2[] _worldSpacePoints;
		VertexPositionColor[] _verts;
		BasicEffect _basicEffect;


		public SimplePolygon( Vector2[] points, Color color )
		{
			_points = points;
			_worldSpacePoints = new Vector2[_points.Length];
			// we need to make tris from the points so it is 3 verts for every tri and we share the first and last points
			_verts = new VertexPositionColor[( _points.Length - 2 ) * 3];

			for( var i = 0; i < _verts.Length; i++ )
				_verts[i].Color = color;

			_basicEffect = new BasicEffect( Core.graphicsDevice );
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}


		public override void onRemovedFromEntity()
		{
			if( _basicEffect != null )
			{
				_basicEffect.Dispose();
				_basicEffect = null;
			}
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( camera.bounds.Intersects( bounds ) )
			{
				_basicEffect.Projection = camera.getProjectionMatrix();
				_basicEffect.View = camera.transformMatrix;
				_basicEffect.CurrentTechnique.Passes[0].Apply();

				Core.graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
				Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _verts, 0, _points.Length - 2, VertexPositionColor.VertexDeclaration );
			}
		}
	
	}
}

