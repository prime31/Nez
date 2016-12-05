using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;
using System.Runtime.CompilerServices;


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
			/// joints are smoothed with some extra geometry. Uses degreesPerSubdivision to decide how smooth to make each joint.
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

		/// <summary>
		/// the type of end cap for all joints
		/// </summary>
		/// <value>The end type of the cap.</value>
		public EndCapType endCapType { get; protected set; } = EndCapType.Standard;

		/// <summary>
		/// used by EndCapType.JaggedWithCutoff to decide what angle to stop creating jagged joints
		/// </summary>
		/// <value>The cutoff angle for end cap subdivision.</value>
		public float cutoffAngleForEndCapSubdivision { get; protected set; } = 90;

		/// <summary>
		/// used by EndCapType.Smooth to decide how often to subdivide and smooth joints
		/// </summary>
		/// <value>The degrees per subdivision.</value>
		public float degreesPerSubdivision { get; protected set; } = 15;

		// temporary storage for the texture if it is set before the BasicEffect is created
		Texture2D _texture;

		bool _useStartEndColors = true;
		Color _startColor = Color.White;
		Color _endColor = Color.White;

		bool _useStartEndWidths = true;
		float _startWidth = 10;
		float _endWidth = 10;
		float _maxWidth = 10;

		FastList<SegmentPoint> _points = new FastList<SegmentPoint>();
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;

		// state required for calculating verts and rendering
		Segment _firstSegment = new Segment();
		Segment _secondSegment = new Segment();
		Segment _lastSegment = new Segment();
		FastList<short> _indices = new FastList<short>( 50 );
		FastList<VertexPositionColorTexture> _vertices = new FastList<VertexPositionColorTexture>( 50 );


		#region configuration

		/// <summary>
		/// sets whether local or world space will be used for rendering. Defaults to world space. Using local space will take into account
		/// all the Transform properties including scale/rotation/position.
		/// </summary>
		/// <returns>The use world space.</returns>
		/// <param name="useWorldSpace">If set to <c>true</c> use world space.</param>
		public LineRenderer setUseWorldSpace( bool useWorldSpace )
		{
			this.useWorldSpace = useWorldSpace;
			return this;
		}


		/// <summary>
		/// sets the texture. Textures should be horizontally tileable. Pass in null to unset the texture.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="texture">Texture.</param>
		public LineRenderer setTexture( Texture2D texture )
		{
			if( _basicEffect != null )
			{
				_basicEffect.Texture = texture;
				_basicEffect.TextureEnabled = texture != null;
			}
			else
			{
				// store this away until the BasicEffect is created
				_texture = texture;
			}

			return this;
		}


		/// <summary>
		/// sets the EndCapType used for rendering the line
		/// </summary>
		/// <returns>The end cap type.</returns>
		/// <param name="endCapType">End cap type.</param>
		public LineRenderer setEndCapType( EndCapType endCapType )
		{
			this.endCapType = endCapType;
			_areVertsDirty = true;
			return this;
		}


		/// <summary>
		/// sets the cutoff angle for use with EndCapType.JaggedWithCutoff. Any angles less than the cutoff angle will have jagged
		/// joints and all others will have standard.
		/// </summary>
		/// <returns>The cutoff angle for end cap subdivision.</returns>
		/// <param name="cutoffAngleForEndCapSubdivision">Cutoff angle for end cap subdivision.</param>
		public LineRenderer setCutoffAngleForEndCapSubdivision( float cutoffAngleForEndCapSubdivision )
		{
			this.cutoffAngleForEndCapSubdivision = cutoffAngleForEndCapSubdivision;
			_areVertsDirty = true;
			return this;
		}


		/// <summary>
		/// sets the number of degrees between each subdivision for use with EndCapType.Smooth
		/// </summary>
		/// <returns>The per subdivision.</returns>
		/// <param name="degreesPerSubdivision">Degrees per subdivision.</param>
		public LineRenderer setDegreesPerSubdivision( float degreesPerSubdivision )
		{
			Assert.isTrue( degreesPerSubdivision > 0, "degreesPerSubdivision must be greater than 0" );
			this.degreesPerSubdivision = degreesPerSubdivision;
			return this;
		}

		/// <summary>
		/// sets the start and end width. If these are set, the individual point widths will be ignored.
		/// </summary>
		/// <returns>The start end widths.</returns>
		/// <param name="startWidth">Start width.</param>
		/// <param name="endWidth">End width.</param>
		public LineRenderer setStartEndWidths( float startWidth, float endWidth )
		{
			_startWidth = startWidth;
			_endWidth = endWidth;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// clears the global start/end widths and goes back to using the individual point widths
		/// </summary>
		/// <returns>The start end widths.</returns>
		public LineRenderer clearStartEndWidths()
		{
			_useStartEndWidths = false;
			return this;
		}


		/// <summary>
		/// sets the start and end color. If these are set, the individual point colors will be ignored.
		/// </summary>
		/// <returns>The start end colors.</returns>
		/// <param name="startColor">Start color.</param>
		/// <param name="endColor">End color.</param>
		public LineRenderer setStartEndColors( Color startColor, Color endColor )
		{
			_startColor = startColor;
			_endColor = endColor;
			_useStartEndColors = true;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// clears the global start/end colors and goes back to using the individual point colors
		/// </summary>
		/// <returns>The start end colors.</returns>
		public LineRenderer clearStartEndColors()
		{
			_useStartEndColors = false;
			return this;
		}


		public LineRenderer setPoints( Vector2[] points )
		{
			_points.reset();
			_points.ensureCapacity( points.Length );
			for( var i = 0; i < points.Length; i++ )
			{
				_points.buffer[i].position = points[i];
				_points.length++;
			}
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// adds a point to the line. If start/end widths are not set each point should have a width set here.
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="point">Point.</param>
		/// <param name="width">Width.</param>
		public LineRenderer addPoint( Vector2 point, float width = 20 )
		{
			_maxWidth = System.Math.Max( _maxWidth, width );

			_points.ensureCapacity();
			_points.buffer[_points.length].position = point;
			_points.buffer[_points.length].width = width;
			_points.length++;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// adds a point to the line. If start/end widths are not set each point should have a width set here. If start/end colors
		/// are not set a color should be set as well.
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="point">Point.</param>
		/// <param name="width">Width.</param>
		/// <param name="color">Color.</param>
		public LineRenderer addPoint( Vector2 point, float width, Color color )
		{
			_maxWidth = System.Math.Max( _maxWidth, width );

			_points.ensureCapacity();
			_points.buffer[_points.length].position = point;
			_points.buffer[_points.length].width = width;
			_points.buffer[_points.length].color = color;
			_points.length++;
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer addPoints( Vector2[] points )
		{
			_points.ensureCapacity( points.Length );
			for( var i = 0; i < points.Length; i++ )
			{
				_points.buffer[_points.length].position = points[i];
				_points.length++;
			}
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// updates a points properties
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		public LineRenderer updatePoint( int index, Vector2 point )
		{
			_points.buffer[index].position = point;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// updates a points properties
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		/// <param name="width">Width.</param>
		public LineRenderer updatePoint( int index, Vector2 point, float width )
		{
			_maxWidth = System.Math.Max( _maxWidth, width );

			_points.buffer[index].position = point;
			_points.buffer[index].width = width;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// updates a points properties
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		/// <param name="width">Width.</param>
		/// <param name="color">Color.</param>
		public LineRenderer updatePoint( int index, Vector2 point, float width, Color color )
		{
			_maxWidth = System.Math.Max( _maxWidth, width );

			_points.buffer[index].position = point;
			_points.buffer[index].width = width;
			_points.buffer[index].color = color;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// clears all the points
		/// </summary>
		/// <returns>The points.</returns>
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

			if( _useStartEndWidths )
				_maxWidth = System.Math.Max( _startWidth, _endWidth );

			// calculate line length first and simulataneously get our min/max points for the bounds
			var lineLength = 0f;
			var halfMaxWidth = _maxWidth * 0.5f;
			_points.buffer[0].lengthFromPreviousPoint = 0;
			for( var i = 0; i < _points.length - 1; i++ )
			{
				var distance = Vector2.Distance( _points.buffer[i].position, _points.buffer[i + 1].position );
				_points.buffer[i + 1].lengthFromPreviousPoint = distance;
				lineLength += distance;

				maxX = Mathf.maxOf( maxX, _points.buffer[i].position.X + halfMaxWidth, _points.buffer[i + 1].position.X + halfMaxWidth );
				minX = Mathf.minOf( minX, _points.buffer[i].position.X - halfMaxWidth, _points.buffer[i + 1].position.X - halfMaxWidth );
				maxY = Mathf.maxOf( maxY, _points.buffer[i].position.Y + halfMaxWidth, _points.buffer[i + 1].position.Y + halfMaxWidth );
				minY = Mathf.minOf( minY, _points.buffer[i].position.Y - halfMaxWidth, _points.buffer[i + 1].position.Y - halfMaxWidth );
			}

			_bounds.x = minX;
			_bounds.y = minY;
			_bounds.width = maxX - minX;
			_bounds.height = maxY - minY;

			// special case: single segment
			if( _points.length == 2 )
			{
				if( _useStartEndWidths )
				{
					_points.buffer[0].width = _startWidth;
					_points.buffer[1].width = _endWidth;
				}

				if( _useStartEndColors )
				{
					_points.buffer[0].color = _startColor;
					_points.buffer[1].color = _endColor;
				}

				_firstSegment.setPoints( ref _points.buffer[0], ref _points.buffer[1] );
				addSingleSegmentLine( ref _firstSegment, _points.buffer[1].color);
				return;
			}

			var distanceSoFar = 0f;
			var fusedPoint = Vector2.Zero;
			var vertIndex = 0;
			var thirdPoint = new SegmentPoint();

			for( var i = 0; i < _points.length - 1; i++ )
			{
				var firstPoint = _points.buffer[i];
				var secondPoint = _points.buffer[i + 1];

				var hasThirdPoint = _points.length > i + 2;
				if( hasThirdPoint )
					thirdPoint = _points.buffer[i + 2];

				// we need the distance along the line of both the first and second points. distanceSoFar will always be for the furthest point
				// which is the previous point before adding the current segment distance.
				var firstPointDistance = distanceSoFar;
				distanceSoFar += secondPoint.lengthFromPreviousPoint;

				var firstPointRatio = firstPointDistance / lineLength;
				var secondPointRatio = distanceSoFar / lineLength;
				var thirdPointRatio = 0f;
				if( hasThirdPoint )
					thirdPointRatio = ( distanceSoFar + thirdPoint.lengthFromPreviousPoint ) / lineLength;

				if( _useStartEndColors )
				{
					ColorExt.lerp( ref _startColor, ref _endColor, out firstPoint.color, firstPointRatio );
					ColorExt.lerp( ref _startColor, ref _endColor, out secondPoint.color, secondPointRatio );

					if( hasThirdPoint )
						ColorExt.lerp( ref _startColor, ref _endColor, out thirdPoint.color, thirdPointRatio );
				}

				if( _useStartEndWidths )
				{
					firstPoint.width = Mathf.lerp( _startWidth, _endWidth, firstPointRatio );
					secondPoint.width = Mathf.lerp( _startWidth, _endWidth, secondPointRatio );

					if( hasThirdPoint )
						thirdPoint.width = Mathf.lerp( _startWidth, _endWidth, thirdPointRatio );
				}


				if( i == 0 )
				{
					_firstSegment.setPoints( ref firstPoint, ref secondPoint );
					_secondSegment.setPoints( ref secondPoint, ref thirdPoint );
				}
				else
				{
					Utils.swap( ref _firstSegment, ref _secondSegment );
					if( hasThirdPoint )
						_secondSegment.setPoints( ref secondPoint, ref thirdPoint );
				}

				// dont recalculate the fusedPoint for the last segment since there will be no third point to work with
				if( hasThirdPoint )
				{
					var shouldFuseBottom = Vector2Ext.isTriangleCCW( firstPoint.position, secondPoint.position, thirdPoint.position );
					_secondSegment.setFusedData( shouldFuseBottom, ref _firstSegment );
				}

				// special care needs to be take with the first segment since it has a different vert count
				if( i == 0 )
					addFirstSegment( ref _firstSegment, ref _secondSegment, ref vertIndex );
				else
					addSegment( ref _firstSegment, ref vertIndex );

				_lastSegment.cloneFrom( ref _firstSegment );
			}
		}


		/// <summary>
		/// special case for just 2 points, one line segment
		/// </summary>
		/// <param name="segment">Segment.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSingleSegmentLine( ref Segment segment, Color nextPointColor )
		{
			_indices.add( 0 );
			_indices.add( 1 );
			_indices.add( 2 );

			_indices.add( 0 );
			_indices.add( 2 );
			_indices.add( 3 );

			addVert( 0, segment.tl, new Vector2( 0, 1 ), _useStartEndColors ? _startColor : segment.point.color );
			addVert( 1, segment.tr, new Vector2( 1, 1 ), nextPointColor );
			addVert( 2, segment.br, new Vector2( 1, 0 ), nextPointColor );
			addVert( 3, segment.bl, new Vector2( 0, 0 ), _useStartEndColors ? _startColor : segment.point.color );
		}


		/// <summary>
		/// the first segment is special since it has no previous verts to connect to so we handle it separately.
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="nextSegment">Next segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addFirstSegment( ref Segment segment, ref Segment nextSegment, ref int vertIndex )
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

			// the tl vert will always be present, as weill the bl
			addVert( vertIndex++, segment.tl, new Vector2( 0, 1 ), segment.point.color );

			if( nextSegment.shouldFuseBottom )
			{
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), segment.nextPoint.color );
				addVert( vertIndex++, nextSegment.point.position, new Vector2( 1, 0.5f ), segment.nextPoint.color );
				addVert( vertIndex++, nextSegment.hasFusedPoint ? nextSegment.fusedPoint : segment.tl, new Vector2( 1, 0 ), segment.nextPoint.color );
			}
			else
			{
				addVert( vertIndex++, nextSegment.hasFusedPoint ? nextSegment.fusedPoint : segment.bl, new Vector2( 1, 1 ), segment.nextPoint.color );
				addVert( vertIndex++, nextSegment.point.position, new Vector2( 1, 0.5f ), segment.nextPoint.color );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), segment.nextPoint.color );
			}

			addVert( vertIndex++, segment.bl, new Vector2( 0, 0 ), segment.point.color );
		}


		/// <summary>
		/// adds a segment and takes care of patching the previous elbow
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addSegment( ref Segment segment, ref int vertIndex )
		{
			// first, we need to patch the previous elbow gap
			patchJoint( ref segment, ref vertIndex );
			
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
				addVert( vertIndex++, segment.tl, new Vector2( 0, 1 ), segment.point.color );
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), segment.nextPoint.color );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), segment.nextPoint.color );
				addVert( vertIndex++, segment.hasFusedPoint ? segment.fusedPoint : segment.bl, new Vector2( 0, 0 ), segment.point.color );
			}
			else
			{
				addVert( vertIndex++, segment.hasFusedPoint ? segment.fusedPoint : segment.tl, new Vector2( 0, 1 ), segment.point.color );
				addVert( vertIndex++, segment.tr, new Vector2( 1, 1 ), segment.nextPoint.color );
				addVert( vertIndex++, segment.br, new Vector2( 1, 0 ), segment.nextPoint.color );
				addVert( vertIndex++, segment.bl, new Vector2( 0, 0 ), segment.point.color );
			}

			addVert( vertIndex++, segment.point.position, new Vector2( 1, 0.5f ), segment.point.color );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void patchJoint( ref Segment segment, ref int vertIndex )
		{
			switch( endCapType )
			{
				case EndCapType.Standard:
					patchStandardJoint( ref segment, ref vertIndex );
					break;
				case EndCapType.Jagged:
					patchJaggedJoint( ref segment, ref vertIndex );
					break;
				case EndCapType.JaggedWithCutoff:
					if( segment.angle < cutoffAngleForEndCapSubdivision )
						patchJaggedJoint( ref segment, ref vertIndex );
					else
						patchStandardJoint( ref segment, ref vertIndex );
					break;
				case EndCapType.Smooth:
					patchSmoothJoint( ref segment, ref vertIndex );
					break;
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void patchStandardJoint( ref Segment segment, ref int vertIndex )
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
		void patchJaggedJoint( ref Segment segment, ref int vertIndex )
		{
			Vector2 intersection;
			if( segment.shouldFuseBottom )
			{
				if( Vector2Ext.getRayIntersection( segment.tl, segment.tr, _lastSegment.tl, _lastSegment.tr, out intersection ) )
				{
					addVert( vertIndex++, intersection, new Vector2( 1, 1 ), segment.point.color );

					_indices.add( (short)vertIndex );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 1 ) );

					_indices.add( (short)( vertIndex - 1 ) );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 5 ) );
				}
			}
			else
			{
				if( Vector2Ext.getRayIntersection( segment.bl, segment.br, _lastSegment.bl, _lastSegment.br, out intersection ) )
				{
					var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
					addVert( vertIndex++, intersection, new Vector2( 1, 0 ), segment.point.color );

					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex + 3 ) );
					_indices.add( (short)( vertIndex - 1 ) );

					_indices.add( (short)( vertIndex - 3 + firstSegmentOffset ) );
					_indices.add( (short)( vertIndex + 4 ) );
					_indices.add( (short)( vertIndex - 1 ) );
				}
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void patchSmoothJoint( ref Segment segment, ref int vertIndex )
		{
			if( segment.shouldFuseBottom )
			{
				// first, we need to get the angle from the point to the tr and tl verts
				var a = _lastSegment.tr;
				var b = segment.tl;
				var center = segment.point.position;

				// we get the angle from 3 o'clock to each of the points, then get the angle in degrees of the pacman shape
				var angle1 = Mathf.atan2( a.Y - center.Y, a.X - center.X ) * Mathf.rad2Deg;
				var angle2 = Mathf.atan2( b.Y - center.Y, b.X - center.X ) * Mathf.rad2Deg;
				var deltaAngle = Mathf.deltaAngle( angle1, angle2 );

				// figure out how many verts we are going to add to the joint
				var totalNewVerts = Mathf.ceil( System.Math.Abs( deltaAngle ) / degreesPerSubdivision );
				var angleIncrement = deltaAngle / ( totalNewVerts + 1 );

				// first triangle will go from the tr vert of the last segment, to the point, to the first new vert
				_indices.add( (short)( vertIndex ) ); // first new vert
				_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // point
				_indices.add( (short)( vertIndex - 4 ) ); // tr of previous

				// add all the triangles that are not connected to either of the two segments
				for( var i = 0; i < totalNewVerts - 1; i++ )
				{
					_indices.add( (short)( vertIndex + i ) ); // prev new vert
					_indices.add( (short)( vertIndex + i + 1 ) ); // next new vert
					_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // point
				}

				// finally, add the last triangle
				_indices.add( (short)( vertIndex + totalNewVerts ) ); // 0	tl of next
				_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // point
				_indices.add( (short)( vertIndex + totalNewVerts - 1 ) ); // last new vert

				// and now we add all the verts using the angleIncrement we calcualted earlier to step from angle1 to angle2
				for( var i = 0; i < totalNewVerts; i++ )
				{
					var midAngle = angle1 + angleIncrement * ( i + 1 );
					var midPoint = Mathf.pointOnCircle( center, segment.point.width / 2, midAngle );
					addVert( vertIndex++, midPoint, new Vector2( 1, 1 ), segment.point.color );
				}
			}
			else
			{
				var a = _lastSegment.br;
				var b = segment.bl;
				var center = segment.point.position;

				var angle1 = Mathf.atan2( a.Y - center.Y, a.X - center.X ) * Mathf.rad2Deg;
				var angle2 = Mathf.atan2( b.Y - center.Y, b.X - center.X ) * Mathf.rad2Deg;
				var deltaAngle = Mathf.deltaAngle( angle1, angle2 );

				var totalNewVerts = Mathf.ceil( System.Math.Abs( deltaAngle ) / degreesPerSubdivision );
				var angleIncrement = deltaAngle / ( totalNewVerts + 1 );

				var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
				_indices.add( (short)( vertIndex - 3 + firstSegmentOffset ) ); // bl of previous
				_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // center
				_indices.add( (short)( vertIndex ) ); // first new vert

				for( var i = 0; i < totalNewVerts - 1; i++ )
				{
					_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // point
					_indices.add( (short)( vertIndex + i + 1 ) ); // next new vert
					_indices.add( (short)( vertIndex + i ) ); // prev new vert
				}

				_indices.add( (short)( vertIndex + 4 + totalNewVerts ) ); // point
				_indices.add( (short)( vertIndex + 3 + totalNewVerts ) ); // br of next
				_indices.add( (short)( vertIndex + totalNewVerts - 1 ) ); // last new vert

				for( var i = 0; i < totalNewVerts; i++ )
				{
					var midAngle = angle1 + angleIncrement * ( i + 1 );
					var midPoint = Mathf.pointOnCircle( center, segment.point.width / 2, midAngle );
					addVert( vertIndex++, midPoint, new Vector2( 1, 0 ), segment.point.color );
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


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			// we dont care if the transform changed if we are in world space
			if( useWorldSpace )
				return;

			_bounds.calculateBounds( entity.transform.position, _localOffset, Vector2.Zero, entity.transform.scale, entity.transform.rotation, width, height );
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
			Core.graphicsDevice.SamplerStates[0] = Core.defaultWrappedSamplerState;
			Core.graphicsDevice.DrawUserIndexedPrimitives( PrimitiveType.TriangleList, _vertices.buffer, 0, _vertices.length, _indices.buffer, 0, primitiveCount );
		}


		public override void debugRender( Graphics graphics )
		{
			for( var i = 0; i < _vertices.length; i++ )
			{
				var v = _vertices[i];
				graphics.batcher.drawPixel( v.Position.X, v.Position.Y, Color.GhostWhite, 4 );
			}

			graphics.batcher.drawHollowRect( _bounds, Debug.Colors.colliderBounds );
		}

		#endregion


		#region Helper classes

		struct SegmentPoint
		{
			public Vector2 position;
			public Color color;
			public float width;
			public float lengthFromPreviousPoint;
		}


		/// <summary>
		/// helper class used to store some data when calculating verts
		/// </summary>
		class Segment
		{
			public Vector2 tl, tr, br, bl;
			public SegmentPoint point;
			public SegmentPoint nextPoint;
			public Vector2 fusedPoint;
			public bool hasFusedPoint;
			public bool shouldFuseBottom;
			public float angle;


			public void setPoints( ref SegmentPoint point, ref SegmentPoint nextPoint )
			{
				angle = 0;
				this.point = point;
				this.nextPoint = nextPoint;

				// rotate 90 degrees before calculating and cache cos/sin
				var radians = Mathf.atan2( nextPoint.position.Y - point.position.Y, nextPoint.position.X - point.position.X );
				radians += MathHelper.PiOver2;
				var halfCos = Mathf.cos( radians ) * 0.5f;
				var halfSin = Mathf.sin( radians ) * 0.5f;

				tl = point.position - new Vector2( point.width * halfCos, point.width * halfSin );
				tr = nextPoint.position - new Vector2( nextPoint.width * halfCos, nextPoint.width * halfSin );
				br = nextPoint.position + new Vector2( nextPoint.width * halfCos, nextPoint.width * halfSin );
				bl = point.position + new Vector2( point.width * halfCos, point.width * halfSin );
			}


			public void setFusedData( bool shouldFuseBottom, ref Segment segment )
			{
				// store the angle off for later. For extreme angles we add extra verts to smooth the joint
				angle = Vector2Ext.angle( segment.point.position - point.position, nextPoint.position - point.position );
				this.shouldFuseBottom = shouldFuseBottom;

				if( shouldFuseBottom )
					hasFusedPoint = ShapeCollisions.lineToLine( segment.bl, segment.br, bl, br, out fusedPoint );
				else
					hasFusedPoint = ShapeCollisions.lineToLine( segment.tl, segment.tr, tl, tr, out fusedPoint );
			}


			public void cloneFrom( ref Segment segment )
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

		#endregion

	}
}

