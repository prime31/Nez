using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// renders a basic, CCW, convex polygon
	/// </summary>
	public class SimplePolygon : RenderableComponent
	{
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleF.rectEncompassingPoints( worldSpacePoints );
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
					if( entity.transform.rotation == 0f )
					{
					    var positionAddition = _localOffset - _origin;
					    if( entity != null )
							positionAddition += entity.transform.position;

						for( var i = 0; i < _points.Length; i++ )
							_worldSpacePoints[i] = _points[i] + positionAddition;
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
				var worldPosX = _localOffset.X;
				var worldPosY = _localOffset.Y;
				
				if( entity != null )
				{
					worldPosX += entity.transform.position.X;
					worldPosY += entity.transform.position.Y;
				}
				
				var tempMat = Matrix.Identity;

				// set the reference point taking origin into account
				_transformMatrix = Matrix.CreateTranslation( -_origin.X, -_origin.Y, 0f ); // origin
				Matrix.CreateRotationZ( entity.transform.rotation, out tempMat ); // rotation
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
		}


		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.contentManager.loadMonoGameEffect<BasicEffect>();
			_basicEffect = new BasicEffect( Core.graphicsDevice );
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			// TODO: set the _basicEffect.World = entity.transform.localToWorldTransform instead of manualy mucking with verts and a local matrix
			// see the deferred lighting PolygonMesh class for details.

			Core.graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
			Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _verts, 0, _points.Length - 2, VertexPositionColor.VertexDeclaration );
		}
	
	}
}

