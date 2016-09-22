using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
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
		public enum EndCapType
		{
			/// <summary>
			/// will not attempt to add any extra verts at joints
			/// </summary>
			Standard,

			/// <summary>
			/// all joints will be extruded out with an extra vert resulting in jagged, pointy joints
			/// </summary>
			Jagged,

			/// <summary>
			/// the same as jagged but uses cutoffAngleForEndCapSubdivision to decide if a joint should be Jagged or Standard
			/// </summary>
			JaggedWithCutoff,

			/// <summary>
			/// joints are smoothed with some extra geometry
			/// </summary>
			Smooth
		}

		public override RectangleF bounds
		{
			// we calculate bounds in update so no need to mess with anything here
			get { return _bounds; }
		}

		/// <summary>
		/// controls whether the lines are defined in world space or local
		/// </summary>
		public bool useWorldSpace { get; protected set; } = true;

		public EndCapType endCapType = EndCapType.Standard;

		/// <summary>
		/// used by EndCapType.JaggedWithCutoff to decide what angle to stop creating jagged joints
		/// </summary>
		/// <value>The cutoff angle for end cap subdivision.</value>
		public float cutoffAngleForEndCapSubdivision { get; private set; } = 90;

		Texture2D _texture;

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

		FastList<Vector2> _points = new FastList<Vector2>();
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;

		// state required for calculating verts and rendering
		Segment _firstSegment = new Segment();
		Segment _secondSegment = new Segment();
		Segment _lastSegment = new Segment();
		FastList<short> _indices = new FastList<short>( 50 );
		FastList<VertexPositionColorTexture> _vertices = new FastList<VertexPositionColorTexture>( 50 );


		#region configuration

		public LineRenderer setUseWorldSpace( bool useWorldSpace )
		{
			this.useWorldSpace = useWorldSpace;
			return this;
		}


		public LineRenderer setTexture( Texture2D texture )
		{
			if( _basicEffect != null )
			{
				_basicEffect.Texture = texture;
				_basicEffect.TextureEnabled = true;
			}
			else
			{
				// store this away until the BasicEffect is created
				_texture = texture;
			}

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


		public LineRenderer setCutoffAngleForEndCapSubdivision( float cutoffAngleForEndCapSubdivision )
		{
			cutoffAngleForEndCapSubdivision = cutoffAngleForEndCapSubdivision;
			return this;
		}


		public LineRenderer setPoints( Vector2[] points )
		{
			_points.reset();
			_points.addRange( points );
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer addPoint( Vector2 point )
		{
			_points.add( point );
			_areVertsDirty = true;
			return this;
		}


		public LineRenderer addPoints( Vector2[] points )
		{
			_points.addRange( points );
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer updatePoint( int index, Vector2 point )
		{
			_points.buffer[index] = point;
			_areVertsDirty = true;
			return this;
		}


		public LineRenderer clearPoints()
		{
			_points.reset();
			_bounds = RectangleF.empty;
			return this;
		}

		#endregion


		void calculateVertices()
		{
			if( !_areVertsDirty || _points.length < 2 )
				return;

			_areVertsDirty = false;
			_indices.reset();
			_vertices.reset();

			var maxX = float.MinValue;
			var minX = float.MaxValue;
			var maxY = float.MinValue;
			var minY = float.MaxValue;

			// calculate line length first and simulataneously get our min/max points for the bounds
			var lineLength = 0f;
			var maxWidth = System.Math.Max( _startWidth, _endWidth ) * 0.5f;
			for( var i = 0; i < _points.length - 1; i++ )
			{
				lineLength += Vector2.Distance( _points[i], _points[i + 1] );
				maxX = Mathf.maxOf( maxX, _points[i].X + maxWidth, _points[i + 1].X + maxWidth );
				minX = Mathf.minOf( minX, _points[i].X - maxWidth, _points[i + 1].X - maxWidth );
				maxY = Mathf.maxOf( maxY, _points[i].Y + maxWidth, _points[i + 1].Y + maxWidth );
				minY = Mathf.minOf( minY, _points[i].Y - maxWidth, _points[i + 1].Y - maxWidth );
			}

			_bounds.x = minX;
			_bounds.y = minY;
			_bounds.width = maxX - minX;
			_bounds.height = maxY - minY;

			var distanceSoFar = 0f;
			var fusedPoint = Vector2.Zero;
			var vertIndex = 0;

			// special case: single segment
			if( _points.length == 2 )
			{
				_firstSegment.setPoints( _points[0], _points[1], _startWidth, _endWidth );
				addSingleSegmentLine( _firstSegment );
				return;
			}

			Debug.log( "----" );
			for( var i = 0; i < _points.length - 1; i++ )
			{
				var firstPoint = _points[i];
				var secondPoint = _points[i + 1];
				Vector2? thirdPoint = null;
				if( _points.length > i + 2 )
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
					Debug.log( "3rd pt has value fucker" );
					var shouldFuseBottom = Vector2Ext.isTriangleCCW( firstPoint, secondPoint, thirdPoint.Value );
					_secondSegment.setFusedData( shouldFuseBottom, _firstSegment );
				}

				// special care needs to be take with the first segment since it has a different vert count
				if( i == 0 )
					addFirstSegment( _firstSegment, _secondSegment, ref vertIndex );
				else
					addSegment( _firstSegment, ref vertIndex );

				_lastSegment.cloneFrom( _firstSegment );
			}
		}


		/// <summary>
		/// special case for just 2 points, one line segment
		/// </summary>
		/// <param name="segment">Segment.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSingleSegmentLine( Segment segment )
		{
			_indices.add( 0 );
			_indices.add( 1 );
			_indices.add( 2 );

			_indices.add( 0 );
			_indices.add( 2 );
			_indices.add( 3 );

			addVert( 0, segment.tl, new Vector2( 0, 1 ), _startColor );
			addVert( 1, segment.tr, new Vector2( 1, 1 ), _endColor );
			addVert( 2, segment.br, new Vector2( 1, 0 ), _endColor );
			addVert( 3, segment.bl, new Vector2( 0, 0 ), _startColor );
		}


		/// <summary>
		/// the first segment is special since it has no previous verts to connect to so we handle it separately.
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="nextSegment">Next segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addFirstSegment( Segment segment, Segment nextSegment, ref int vertIndex )
		{
			_indices.add( 0 );
			_indices.add( 1 );
			_indices.add( 4 );

			_indices.add( 1 );
			_indices.add( 2 );
			_indices.add( 4 );

			_indices.add( 2 );
			_indices.add( 3 );
			_indices.add( 4 );

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


		/// <summary>
		/// adds a segment and takes care of patching the previous elbow
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSegment( Segment segment, ref int vertIndex )
		{
			// first, we need to patch the previous elbow gap
			patchJoint( segment, ref vertIndex );
			
			_indices.add( (short)vertIndex );
			_indices.add( (short)( vertIndex + 1 ) );
			_indices.add( (short)( vertIndex + 2 ) );

			_indices.add( (short)( vertIndex + 4 ) );
			_indices.add( (short)vertIndex );
			_indices.add( (short)( vertIndex + 2 ) );

			_indices.add( (short)( vertIndex + 3 ) );
			_indices.add( (short)( vertIndex + 4 ) );
			_indices.add( (short)( vertIndex + 2 ) );

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
		void patchJoint( Segment segment, ref int vertIndex )
		{
			switch( endCapType )
			{
				case EndCapType.Standard:
					patchStandardJoint( segment, ref vertIndex );
					break;
				case EndCapType.Jagged:
					patchJaggedJoint( segment, ref vertIndex );
					break;
				case EndCapType.JaggedWithCutoff:
					if( segment.angle < cutoffAngleForEndCapSubdivision )
						patchJaggedJoint( segment, ref vertIndex );
					else
						patchStandardJoint( segment, ref vertIndex );
					break;
				case EndCapType.Smooth:
					break;
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void patchStandardJoint( Segment segment, ref int vertIndex )
		{
			if( segment.shouldFuseBottom )
			{
				_indices.add( (short)vertIndex );
				_indices.add( (short)( vertIndex + 4 ) );
				_indices.add( (short)( vertIndex - 4 ) );
			}
			else
			{
				// If this is the second segment we need a different vert from the first segment since the first segment has 1 less vert than
				// all mid segments.
				var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
				_indices.add( (short)( vertIndex - 3 + firstSegmentOffset ) );
				_indices.add( (short)( vertIndex + 4 ) );
				_indices.add( (short)( vertIndex + 3 ) );
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void patchJaggedJoint( Segment segment, ref int vertIndex )
		{
			Vector2 intersection;
			if( segment.shouldFuseBottom )
			{
				if( Vector2Ext.getRayIntersection( segment.tl, segment.tr, _lastSegment.tl, _lastSegment.tr, out intersection ) )
				{
					addVert( vertIndex++, intersection, new Vector2( 1, 1 ), _endColor );

					_indices.add( (short)vertIndex );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 1 ) );

					_indices.add( (short)( vertIndex - 1 ) );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 6 ) );
				}
			}
			else
			{
				if( Vector2Ext.getRayIntersection( segment.bl, segment.br, _lastSegment.bl, _lastSegment.br, out intersection ) )
				{
					var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
					addVert( vertIndex++, intersection, new Vector2( 1, 0 ), _endColor );

					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex + 3 ) );
					_indices.add( (short)( vertIndex - 1 ) );

					_indices.add( (short)( vertIndex - 3 + firstSegmentOffset ) );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 1 ) );

					Core.graphicsDevice.RasterizerState = RasterizerState.CullNone;
				}
			}
		}


		/// <summary>
		/// adds a vert to the list
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="position">Position.</param>
		/// <param name="texCoord">Tex coordinate.</param>
		/// <param name="col">Col.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addVert( int index, Vector2 position, Vector2 texCoord, Color col )
		{
			_vertices.ensureCapacity();
			_vertices.buffer[index].Position = position.toVector3();
			_vertices.buffer[index].TextureCoordinate = texCoord;
			_vertices.buffer[index].Color = col;
			_vertices.length++;
		}


		#region Component/RenderableComponent

		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.content.loadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;

			if( _texture != null )
			{
				_basicEffect.Texture = _texture;
				_basicEffect.TextureEnabled = true;
				_texture = null;
			}
		}


		public override void onEntityTransformChanged()
		{
			// we dont care if the transform changed if we are in world space
			if( useWorldSpace )
				return;

			_bounds.calculateBounds( entity.transform.position, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, width, height );
		}


		public override bool isVisibleFromCamera( Camera camera )
		{
			calculateVertices();
			return base.isVisibleFromCamera( camera );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( _points.length < 2 )
				return;
			
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			if( !useWorldSpace )
				_basicEffect.World = transform.localToWorldTransform;

			var primitiveCount = _indices.length / 3;
			Core.graphicsDevice.DrawUserIndexedPrimitives( PrimitiveType.TriangleList, _vertices.buffer, 0, _vertices.length, _indices.buffer, 0, primitiveCount );
		}

		#endregion


		/// <summary>
		/// helper class used to store some data when calculating verts
		/// </summary>
		class Segment
		{
			public Vector2 tl, tr, br, bl;
			public Vector2 point;
			public Vector2 nextPoint;
			public Vector2 fusedPoint;
			public bool hasFusedPoint;
			public bool shouldFuseBottom;
			public float angle;


			public void setPoints( Vector2 point, Vector2 nextPoint, float firstPointWidth, float secondPointWidth )
			{
				angle = 0;
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
				// store the angle off for later. For extreme angles we add extra verts to smooth the joint
				angle = Vector2Ext.angle( segment.point - point, nextPoint - point );
				this.shouldFuseBottom = shouldFuseBottom;

				if( shouldFuseBottom )
					hasFusedPoint = ShapeCollisions.lineToLine( segment.bl, segment.br, bl, br, out fusedPoint );
				else
					hasFusedPoint = ShapeCollisions.lineToLine( segment.tl, segment.tr, tl, tr, out fusedPoint );
			}


			public void cloneFrom( Segment segment )
			{
				tl = segment.tl;
				tr = segment.tr;
				br = segment.br;
				bl = segment.bl;
				point = segment.point;
				nextPoint = segment.nextPoint;
				hasFusedPoint = segment.hasFusedPoint;
				shouldFuseBottom = segment.shouldFuseBottom;
				angle = segment.angle;
			}

		}

	}
}

