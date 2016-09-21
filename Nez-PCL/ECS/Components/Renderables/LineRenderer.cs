using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.PhysicsShapes;
using System.Runtime.CompilerServices;


// TODO:
//			cache distances
//			?? allow width/Color per point ??

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

		Vector2[] _points;
		VertexPositionColorTexture[] _vertices;
		short[] _triangleIndices;
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;

		// state
		Segment _firstSegment = new Segment();
		Segment _secondSegment = new Segment();
		List<short> _indices = new List<short>();


		#region configuration

		public LineRenderer setPointCount( int pointCount )
		{
			Assert.isTrue( pointCount > 1, "A line must have at least two points" );
			_pointCount = pointCount;

			// ensure we have room for all the positions
			if( _points == null || _points.Length != pointCount )
			{
				_points = new Vector2[pointCount];
				_vertices = null;
			}

			// we need 5 verts for each point except the last point which needs no verts
			_vertices = new VertexPositionColorTexture[calculateTotalVerts()];
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


		public LineRenderer setPoint( int index, Vector2 point )
		{
			Assert.isNotNull( _points, "points array is null. Did you call setPositionCount first?" );

			_points[index] = point;
			_areVertsDirty = true;
			return this;
		}


		public LineRenderer setPoints( Vector2[] points )
		{
			if( points.Length > _pointCount )
				setPointCount( points.Length );
			
			_points = points;
			_areVertsDirty = true;

			return this;
		}

		#endregion


		/// <summary>
		/// Calculates the total verts
		/// </summary>
		/// <returns>The total verts.</returns>
		int calculateTotalVerts()
		{
			var mid = ( _pointCount - 2 ) * 6;
			var caps = 2 * 5;

			return mid + caps;
		}


		public static float angle( Vector2 from, Vector2 to )
		{
			Vector2Ext.normalize( ref from );
			Vector2Ext.normalize( ref to );
			return Mathf.acos( Mathf.clamp( Vector2.Dot( from, to ), -1f, 1f ) ) * Mathf.rad2Deg;
		}


		void calculateVertices()
		{
			if( !_areVertsDirty || _pointCount == 0 )
				return;

			_areVertsDirty = false;
			_indices.Clear();

			var maxX = float.MinValue;
			var minX = float.MaxValue;
			var maxY = float.MinValue;
			var minY = float.MaxValue;

			// calculate line length first
			var lineLength = 0f;
			for( var i = 0; i < _pointCount - 1; i++ )
			{
				var ang = angle( _points[i], _points[i + 1] );
				lineLength += Vector2.Distance( _points[i], _points[i + 1] );
			}

			var distanceSoFar = 0f;
			var fusedPoint = Vector2.Zero;
			var vertIndex = 0;

			// special case: single segment
			if( _pointCount == 2 )
			{
				_firstSegment.setPoints( _points[0], _points[1], _startWidth, _endWidth );
				addSingleSegmentLine( _firstSegment );
				_triangleIndices = _indices.ToArray();

				// update min/max for any visible verts
				maxX = Mathf.maxOf( maxX, _firstSegment.tl.X, _firstSegment.tr.X, _firstSegment.br.X, _firstSegment.bl.X );
				minX = Mathf.minOf( minX, _firstSegment.tl.X, _firstSegment.tr.X, _firstSegment.br.X, _firstSegment.bl.X );
				maxY = Mathf.maxOf( maxY, _firstSegment.tl.Y, _firstSegment.tr.Y, _firstSegment.br.Y, _firstSegment.bl.Y );
				minY = Mathf.minOf( minY, _firstSegment.tl.Y, _firstSegment.tr.Y, _firstSegment.br.Y, _firstSegment.bl.Y );

				_bounds.x = minX;
				_bounds.y = minY;
				_bounds.width = maxX - minX;
				_bounds.height = maxY - minY;
				return;
			}


			for( var i = 0; i < _pointCount - 1; i++ )
			{
				var firstPoint = _points[i];
				var secondPoint = _points[i + 1];
				Vector2? thirdPoint = null;
				if( _pointCount > i + 2 )
					thirdPoint = _points[i + 2];

				// we need the distance along the line of both the first and second points. distanceSoFar will always be for the furthest point
				// which is the previous point before adding the current segment distance.
				var firstPointDistance = distanceSoFar;
				distanceSoFar += Vector2.Distance( firstPoint, secondPoint );

				var firstPointRatio = firstPointDistance / lineLength;
				var secondPointRatio = distanceSoFar / lineLength;

				Color firstPointColor, secondPointColor;
				ColorExt.lerp( ref _startColor, ref _endColor, out firstPointColor, firstPointRatio );
				ColorExt.lerp( ref _startColor, ref _endColor, out secondPointColor, secondPointRatio );

				var firstPointWidth = Mathf.lerp( _startWidth, _endWidth, firstPointRatio );
				var secondPointWidth = Mathf.lerp( _startWidth, _endWidth, secondPointRatio );


				if( i == 0 )
				{
					_firstSegment.setPoints( firstPoint, secondPoint, firstPointWidth, secondPointWidth );
					_secondSegment.setPoints( secondPoint, thirdPoint.Value, firstPointWidth, secondPointWidth );
				}
				else
				{
					Utils.swap( ref _firstSegment, ref _secondSegment );
					if( thirdPoint.HasValue )
						_secondSegment.setPoints( secondPoint, thirdPoint.Value, firstPointWidth, secondPointWidth );
				}

				// dont recalculate the fusedPoint for the last segment since there will be no third point to work with
				if( thirdPoint.HasValue )
				{
					var shouldFuseBottom = Vector2Ext.isTriangleCCW( firstPoint, secondPoint, thirdPoint.Value );
					_secondSegment.setFusedData( shouldFuseBottom, _firstSegment );
				}

				// special care needs to be take with the first segment since it has a different vert count
				if( i == 0 )
					addFirstSegment( _firstSegment, _secondSegment, ref vertIndex );
				else
					addSegment( _firstSegment, ref vertIndex );

				// update min/max for any visible verts
				maxX = Mathf.maxOf( maxX, _firstSegment.tl.X, _firstSegment.tr.X, _firstSegment.br.X, _firstSegment.bl.X );
				minX = Mathf.minOf( minX, _firstSegment.tl.X, _firstSegment.tr.X, _firstSegment.br.X, _firstSegment.bl.X );
				maxY = Mathf.maxOf( maxY, _firstSegment.tl.Y, _firstSegment.tr.Y, _firstSegment.br.Y, _firstSegment.bl.Y );
				minY = Mathf.minOf( minY, _firstSegment.tl.Y, _firstSegment.tr.Y, _firstSegment.br.Y, _firstSegment.bl.Y );
			}

			_triangleIndices = _indices.ToArray();

			_bounds.x = minX;
			_bounds.y = minY;
			_bounds.width = maxX - minX;
			_bounds.height = maxY - minY;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSingleSegmentLine( Segment segment )
		{
			_indices.Add( 0 );
			_indices.Add( 1 );
			_indices.Add( 2 );

			_indices.Add( 0 );
			_indices.Add( 2 );
			_indices.Add( 3 );

			addVert( 0, segment.tl, new Vector2( 0, 1 ), _startColor );
			addVert( 1, segment.tr, new Vector2( 1, 1 ), _endColor );
			addVert( 2, segment.br, new Vector2( 1, 0 ), _endColor );
			addVert( 3, segment.bl, new Vector2( 0, 0 ), _startColor );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addFirstSegment( Segment segment, Segment nextSegment, ref int vertIndex )
		{
			_indices.Add( 0 );
			_indices.Add( 1 );
			_indices.Add( 4 );

			_indices.Add( 1 );
			_indices.Add( 2 );
			_indices.Add( 4 );

			_indices.Add( 2 );
			_indices.Add( 3 );
			_indices.Add( 4 );

			addVert( vertIndex++, segment.tl, new Vector2( 0, 1 ), _startColor );

			if( nextSegment.shouldFuseBottom )
			{
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), _endColor );
				addVert( vertIndex++, nextSegment.point, new Vector2( 1, 0.5f ), _endColor );
				addVert( vertIndex++, nextSegment.fusedPoint, new Vector2( 1, 0 ), _endColor );
			}
			else
			{
				addVert( vertIndex++, nextSegment.fusedPoint, new Vector2( 1, 1 ), _endColor );
				addVert( vertIndex++, nextSegment.point, new Vector2( 1, 0.5f ), _endColor );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), _startColor );
			}

			addVert( vertIndex++, segment.bl, new Vector2( 0, 0 ), _startColor );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSegment( Segment segment, ref int vertIndex )
		{
			// first, we need to patch the previous elbow gap. If this is the second segment we need a different vert from the first segment
			// since the first segment has 1 less vert than all mid segments.
			//if( segment.hasFusedPoint )
			{
				if( segment.shouldFuseBottom )
				{
					_indices.Add( (short)vertIndex );
					_indices.Add( (short)( vertIndex + 4 ) );
					_indices.Add( (short)( vertIndex - 4 ) );
				}
				else
				{
					var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
					_indices.Add( (short)( vertIndex - 3 + firstSegmentOffset ) );
					_indices.Add( (short)( vertIndex + 4 ) );
					_indices.Add( (short)( vertIndex + 3 ) );
				}
			}
			
			_indices.Add( (short)vertIndex );
			_indices.Add( (short)( vertIndex + 1 ) );
			_indices.Add( (short)( vertIndex + 2 ) );

			_indices.Add( (short)( vertIndex + 4 ) );
			_indices.Add( (short)vertIndex );
			_indices.Add( (short)( vertIndex + 2 ) );

			_indices.Add( (short)( vertIndex + 3 ) );
			_indices.Add( (short)( vertIndex + 4 ) );
			_indices.Add( (short)( vertIndex + 2 ) );

			if( segment.shouldFuseBottom )
			{
				addVert( vertIndex++, segment.tl, new Vector2( 0, 1 ), _startColor );
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), _endColor );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), _startColor );
				addVert( vertIndex++, segment.hasFusedPoint ? segment.fusedPoint : segment.bl, new Vector2( 0, 0 ), _startColor );
			}
			else
			{
				addVert( vertIndex++, segment.hasFusedPoint ? segment.fusedPoint : segment.tl, new Vector2( 0, 1 ), _startColor );
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), _endColor );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), _startColor );
				addVert( vertIndex++, segment.bl, new Vector2( 0, 0 ), _startColor );
			}

			addVert( vertIndex++, segment.point, new Vector2( 1, 0.5f ), _startColor );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addVert( int index, Vector2 position, Vector2 texCoord, Color col )
		{
			if( position == Vector2.Zero )
				Debug.log( "fuck you" );
			
			_vertices[index].Position = position.toVector3();
			_vertices[index].TextureCoordinate = texCoord;
			_vertices[index].Color = col;
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
			public Vector2 tl, tr, br, bl;
			public Vector2 point;
			public Vector2 nextPoint;
			public Vector2 fusedPoint;
			public bool hasFusedPoint;
			public bool shouldFuseBottom;


			public void setPoints( Vector2 point, Vector2 nextPoint, float firstPointWidth, float secondPointWidth )
			{
				this.point = point;
				this.nextPoint = nextPoint;

				// rotate 90 degrees before calculating and cache cos/sin
				var radians = Mathf.atan2( nextPoint.Y - point.Y, nextPoint.X - point.X );
				radians += MathHelper.PiOver2;
				var halfCos = Mathf.cos( radians ) * 0.5f;
				var halfSin = Mathf.sin( radians ) * 0.5f;

				tl = point - new Vector2( firstPointWidth * halfCos, firstPointWidth * halfSin );
				tr = nextPoint - new Vector2( secondPointWidth * halfCos, secondPointWidth * halfSin );
				br = nextPoint + new Vector2( secondPointWidth * halfCos, secondPointWidth * halfSin );
				bl = point + new Vector2( firstPointWidth * halfCos, firstPointWidth * halfSin );
			}


			public void setFusedData( bool shouldFuseBottom, Segment segment )
			{
				this.shouldFuseBottom = shouldFuseBottom;

				if( shouldFuseBottom )
					hasFusedPoint = ShapeCollisions.lineToLine( segment.bl, segment.br, bl, br, out fusedPoint );
				else
					hasFusedPoint = ShapeCollisions.lineToLine( segment.tl, segment.tr, tl, tr, out fusedPoint );
			}

		}

	}
}

