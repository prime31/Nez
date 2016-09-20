using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


// TODO:	move isCCW to Vector2Ext
//			cache distances
//			remove List for triangle indices
//			make points Vector2
//			?? allow width per point ??

namespace Nez
{
	/// <summary>
	/// Renders a trail behind a moving object
	/// Adapted from http://www.paradeofrain.com/2010/01/28/update-on-continuous-2d-trails-in-xna/
	/// </summary>
	public class LineRenderer : RenderableComponent
	{
		public override RectangleF bounds
		{
			// we calculate bounds in update so no need to mess with anything here
			get { return _bounds; }
		}

		/// <summary>
		/// starting color of the ribbon
		/// </summary>
		Color _startColor = Color.White;

		/// <summary>
		/// end (tail) color of the ribbon
		/// </summary>
		Color _endColor = Color.White;

		float _startWidth = 10;
		float _endWidth = 10;

		/// <summary>
		/// number of points in the line
		/// </summary>
		int _pointCount = 0;

		Vector3[] _points;
		VertexPositionColorTexture[] _vertices;
		short[] _triangleIndices;
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;


		#region configuration

		public LineRenderer setPointCount( int pointCount )
		{
			_pointCount = pointCount;

			// ensure we have room for all the positions
			if( _points == null || _points.Length != pointCount )
			{
				_points = new Vector3[pointCount];
				_vertices = null;
			}

			// we need 5 verts for each point except the last point which needs no verts
			if( _pointCount > 0 )
				_vertices = new VertexPositionColorTexture[( pointCount - 1 ) * 5];
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer setWidth( float startWidth, float endWidth )
		{
			_startWidth = startWidth;
			_endWidth = endWidth;
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer setColors( Color startColor, Color endColor )
		{
			_startColor = startColor;
			_endColor = endColor;
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer setPoint( int index, Vector3 point )
		{
			Assert.isNotNull( _points, "points array is null. Did you call setPositionCount first?" );

			_points[index] = point;
			_areVertsDirty = true;
			return this;
		}


		public LineRenderer setPoints( Vector3[] points )
		{
			if( points.Length > _pointCount )
				setPointCount( points.Length );
			
			_points = points;
			_areVertsDirty = true;

			return this;
		}

		#endregion


		void calculateVertices()
		{
			if( !_areVertsDirty || _pointCount == 0 )
				return;

			_areVertsDirty = false;

			var maxX = float.MinValue;
			var minX = float.MaxValue;
			var maxY = float.MinValue;
			var minY = float.MaxValue;

			// calculate line length first
			var lineLength = 0f;
			for( var i = 0; i < _pointCount - 1; i++ )
				lineLength += Vector3.Distance( _points[i], _points[i + 1] );


			var firstSegment = new Segment();
			var secondSegment = new Segment();

			var indices = new List<short>();
			var distanceSoFar = 0f;
			for( var i = 0; i < _pointCount - 1; i++ )
			{
				var firstPoint = _points[i];
				var secondPoint = _points[i + 1];
				var vertIndex = i * 5;

				// we need the distance along the line of both the first and second points. distanceSoFar will always be for the furthest point
				// which is the previous point before adding the current segment distance.
				var firstPointDistance = distanceSoFar;
				distanceSoFar += Vector3.Distance( firstPoint, secondPoint );

				var firstPointRatio = firstPointDistance / lineLength;
				var secondPointRatio = distanceSoFar / lineLength;

				Color firstPointColor, secondPointColor;
				ColorExt.lerp( ref _startColor, ref _endColor, out firstPointColor, firstPointRatio );
				ColorExt.lerp( ref _startColor, ref _endColor, out secondPointColor, secondPointRatio );

				var firstPointWidth = Mathf.lerp( _startWidth, _endWidth, firstPointRatio );
				var secondPointWidth = Mathf.lerp( _startWidth, _endWidth, secondPointRatio );


				if( i == 0 )
				{
					firstSegment.setPoints( _points[i], _points[i + 1], firstPointWidth, secondPointWidth );
				}
				else
				{
					Utils.swap( ref firstSegment, ref secondSegment );
					secondSegment.setPoints( _points[i], _points[i + 1], firstPointWidth, secondPointWidth );
					secondSegment.processVerts( firstSegment );
				}


				// rotate 90 degrees before calculating and cache cos/sin
				var radians = Mathf.atan2( secondPoint.Y - firstPoint.Y, secondPoint.X - firstPoint.X );
				radians += MathHelper.PiOver2;
				var halfCos = Mathf.cos( radians ) * 0.5f;
				var halfSin = Mathf.sin( radians ) * 0.5f;

				var tl = firstPoint - new Vector3( firstPointWidth * halfCos, firstPointWidth * halfSin, 0 );
				var tr = secondPoint - new Vector3( secondPointWidth * halfCos, secondPointWidth * halfSin, 0 );
				var br = secondPoint + new Vector3( secondPointWidth * halfCos, secondPointWidth * halfSin, 0 );
				var bl = firstPoint + new Vector3( firstPointWidth * halfCos, firstPointWidth * halfSin, 0 );


				indices.Add( (short)vertIndex );
				indices.Add( (short)( vertIndex + 1 ) );
				indices.Add( (short)( vertIndex + 2 ) );

				indices.Add( (short)vertIndex );
				indices.Add( (short)( vertIndex + 2 ) );
				indices.Add( (short)( vertIndex + 3 ) );

				// tl
				_vertices[vertIndex].Position = tl;
				_vertices[vertIndex].TextureCoordinate = new Vector2( 0, 1 );
				_vertices[vertIndex++].Color = firstPointColor;

				// tr
				_vertices[vertIndex].Position = tr;
				_vertices[vertIndex].TextureCoordinate = new Vector2( 1, 1 );
				_vertices[vertIndex++].Color = secondPointColor;

				// br
				_vertices[vertIndex].Position = br;
				_vertices[vertIndex].TextureCoordinate = new Vector2( 1, 0 );
				_vertices[vertIndex++].Color = secondPointColor;

				// bl
				_vertices[vertIndex].Position = bl;
				_vertices[vertIndex].TextureCoordinate = new Vector2( 0, 0 );
				_vertices[vertIndex++].Color = firstPointColor;

				// first point
				_vertices[vertIndex].Position = firstPoint;
				_vertices[vertIndex].TextureCoordinate = new Vector2( 0, 0.5f );
				_vertices[vertIndex++].Color = firstPointColor;

				// for all segments except the first we need to clean up the joint/elbow by adding a new vert and triangle to fill the gap
				if( i != 0 )
				{
					// we need to fill the bottom gap when we have a counter-clockwise angle or the top for clockwise
					var prevPoint = _points[i - 1];

					if( isCCW( prevPoint, firstPoint, secondPoint ) )
					{
						indices.Add( (short)( vertIndex - 1 ) );
						indices.Add( (short)( vertIndex - 9 ) ); // tr
						indices.Add( (short)( vertIndex - 5 ) ); // tl
					}
					else
					{
						indices.Add( (short)( vertIndex - 1 ) );
						indices.Add( (short)( vertIndex - 2 ) ); // bl
						indices.Add( (short)( vertIndex - 8 ) ); // br
					}
				}

				// update min/max for any visible verts
				maxX = Mathf.maxOf( maxX, tl.X, tr.X, br.X, bl.X );
				minX = Mathf.minOf( minX, tl.X, tr.X, br.X, bl.X );
				maxY = Mathf.maxOf( maxY, tl.Y, tr.Y, br.Y, bl.Y );
				minY = Mathf.minOf( minY, tl.Y, tr.Y, br.Y, bl.Y );
			}

			_triangleIndices = indices.ToArray();

			_bounds.x = minX;
			_bounds.y = minY;
			_bounds.width = maxX - minX;
			_bounds.height = maxY - minY;
		}


		bool isCCW( Vector3 p0, Vector3 p1, Vector3 p2 )
		{
			return Triangulator.isTriangleCCW( p0.toVector2(), p1.toVector2(), p2.toVector2() );
		}


		#region Component/RenderableComponent

		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.content.loadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
			_basicEffect.TextureEnabled = true;

			_basicEffect.Texture = entity.scene.content.Load<Texture2D>( "Images/rainbow" );
		}


		public override bool isVisibleFromCamera( Camera camera )
		{
			calculateVertices();
			return base.isVisibleFromCamera( camera );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( _pointCount == 0 )
				return;
			
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			//Core.graphicsDevice.RasterizerState = RasterizerState.CullNone;
			//Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _vertices, 0, ( _pointCount - 1 ) * 2 );

			var numVerts = _vertices.Length;
			var primitiveCount = _triangleIndices.Length / 3;
			Core.graphicsDevice.DrawUserIndexedPrimitives( PrimitiveType.TriangleList, _vertices, 0, numVerts, _triangleIndices, 0, primitiveCount );
		}

		#endregion


		class Segment
		{
			public Vector3 tl, tr, br, bl;
			public Vector3 point;


			public void setPoints( Vector3 point, Vector3 nextPoint, float firstPointWidth, float secondPointWidth )
			{
				this.point = point;

				// rotate 90 degrees before calculating and cache cos/sin
				var radians = Mathf.atan2( nextPoint.Y - point.Y, nextPoint.X - point.X );
				radians += MathHelper.PiOver2;
				var halfCos = Mathf.cos( radians ) * 0.5f;
				var halfSin = Mathf.sin( radians ) * 0.5f;

				tl = point - new Vector3( firstPointWidth * halfCos, firstPointWidth * halfSin, 0 );
				tr = nextPoint - new Vector3( secondPointWidth * halfCos, secondPointWidth * halfSin, 0 );
				br = nextPoint + new Vector3( secondPointWidth * halfCos, secondPointWidth * halfSin, 0 );
				bl = point + new Vector3( firstPointWidth * halfCos, firstPointWidth * halfSin, 0 );
			}


			public void processVerts( Segment segment )
			{
				
			}

		}

	}
}

