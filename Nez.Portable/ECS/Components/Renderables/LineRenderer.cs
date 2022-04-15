using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;
using System.Runtime.CompilerServices;


namespace Nez
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

	/// <summary>
	/// Renders a trail behind a moving object
	/// Adapted from http://www.paradeofrain.com/2010/01/28/update-on-continuous-2d-trails-in-xna/
	/// </summary>
	public class LineRenderer : RenderableComponent
	{
		public override RectangleF Bounds => _bounds;

		/// <summary>
		/// controls whether the lines are defined in world space or local
		/// </summary>
		public bool UseWorldSpace { get; protected set; } = true;

		/// <summary>
		/// the type of end cap for all joints
		/// </summary>
		/// <value>The end type of the cap.</value>
		public EndCapType EndCapType { get; protected set; } = EndCapType.Standard;

		/// <summary>
		/// used by EndCapType.JaggedWithCutoff to decide what angle to stop creating jagged joints
		/// </summary>
		/// <value>The cutoff angle for end cap subdivision.</value>
		public float CutoffAngleForEndCapSubdivision { get; protected set; } = 90;

		/// <summary>
		/// used by EndCapType.Smooth to decide how often to subdivide and smooth joints
		/// </summary>
		/// <value>The degrees per subdivision.</value>
		public float DegreesPerSubdivision { get; protected set; } = 15;

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
		FastList<short> _indices = new FastList<short>(50);
		FastList<VertexPositionColorTexture> _vertices = new FastList<VertexPositionColorTexture>(50);


		#region configuration

		/// <summary>
		/// sets whether local or world space will be used for rendering. Defaults to world space. Using local space will take into account
		/// all the Transform properties including scale/rotation/position.
		/// </summary>
		/// <returns>The use world space.</returns>
		/// <param name="useWorldSpace">If set to <c>true</c> use world space.</param>
		public LineRenderer SetUseWorldSpace(bool useWorldSpace)
		{
			UseWorldSpace = useWorldSpace;
			return this;
		}


		/// <summary>
		/// sets the texture. Textures should be horizontally tileable. Pass in null to unset the texture.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="texture">Texture.</param>
		public LineRenderer SetTexture(Texture2D texture)
		{
			if (_basicEffect != null)
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
		public LineRenderer SetEndCapType(EndCapType endCapType)
		{
			EndCapType = endCapType;
			_areVertsDirty = true;
			return this;
		}


		/// <summary>
		/// sets the cutoff angle for use with EndCapType.JaggedWithCutoff. Any angles less than the cutoff angle will have jagged
		/// joints and all others will have standard.
		/// </summary>
		/// <returns>The cutoff angle for end cap subdivision.</returns>
		/// <param name="cutoffAngleForEndCapSubdivision">Cutoff angle for end cap subdivision.</param>
		public LineRenderer SetCutoffAngleForEndCapSubdivision(float cutoffAngleForEndCapSubdivision)
		{
			CutoffAngleForEndCapSubdivision = cutoffAngleForEndCapSubdivision;
			_areVertsDirty = true;
			return this;
		}


		/// <summary>
		/// sets the number of degrees between each subdivision for use with EndCapType.Smooth
		/// </summary>
		/// <returns>The per subdivision.</returns>
		/// <param name="degreesPerSubdivision">Degrees per subdivision.</param>
		public LineRenderer SetDegreesPerSubdivision(float degreesPerSubdivision)
		{
			Insist.IsTrue(degreesPerSubdivision > 0, "degreesPerSubdivision must be greater than 0");
			DegreesPerSubdivision = degreesPerSubdivision;
			return this;
		}

		/// <summary>
		/// sets the start and end width. If these are set, the individual point widths will be ignored.
		/// </summary>
		/// <returns>The start end widths.</returns>
		/// <param name="startWidth">Start width.</param>
		/// <param name="endWidth">End width.</param>
		public LineRenderer SetStartEndWidths(float startWidth, float endWidth)
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
		public LineRenderer ClearStartEndWidths()
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
		public LineRenderer SetStartEndColors(Color startColor, Color endColor)
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
		public LineRenderer ClearStartEndColors()
		{
			_useStartEndColors = false;
			return this;
		}


		public LineRenderer SetPoints(Vector2[] points)
		{
			_points.Reset();
			_points.EnsureCapacity(points.Length);
			for (var i = 0; i < points.Length; i++)
			{
				_points.Buffer[i].Position = points[i];
				_points.Length++;
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
		public LineRenderer AddPoint(Vector2 point, float width = 20)
		{
			_maxWidth = System.Math.Max(_maxWidth, width);

			_points.EnsureCapacity();
			_points.Buffer[_points.Length].Position = point;
			_points.Buffer[_points.Length].Width = width;
			_points.Length++;
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
		public LineRenderer AddPoint(Vector2 point, float width, Color color)
		{
			_maxWidth = System.Math.Max(_maxWidth, width);

			_points.EnsureCapacity();
			_points.Buffer[_points.Length].Position = point;
			_points.Buffer[_points.Length].Width = width;
			_points.Buffer[_points.Length].Color = color;
			_points.Length++;
			_areVertsDirty = true;

			return this;
		}


		public LineRenderer AddPoints(Vector2[] points)
		{
			_points.EnsureCapacity(points.Length);
			for (var i = 0; i < points.Length; i++)
			{
				_points.Buffer[_points.Length].Position = points[i];
				_points.Length++;
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
		public LineRenderer UpdatePoint(int index, Vector2 point)
		{
			_points.Buffer[index].Position = point;
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
		public LineRenderer UpdatePoint(int index, Vector2 point, float width)
		{
			_maxWidth = System.Math.Max(_maxWidth, width);

			_points.Buffer[index].Position = point;
			_points.Buffer[index].Width = width;
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
		public LineRenderer UpdatePoint(int index, Vector2 point, float width, Color color)
		{
			_maxWidth = System.Math.Max(_maxWidth, width);

			_points.Buffer[index].Position = point;
			_points.Buffer[index].Width = width;
			_points.Buffer[index].Color = color;
			_areVertsDirty = true;

			return this;
		}


		/// <summary>
		/// clears all the points
		/// </summary>
		/// <returns>The points.</returns>
		public LineRenderer ClearPoints()
		{
			_points.Reset();
			_bounds = RectangleF.Empty;
			return this;
		}

		#endregion


		void CalculateVertices()
		{
			if (!_areVertsDirty || _points.Length < 2)
				return;

			_areVertsDirty = false;
			_indices.Reset();
			_vertices.Reset();

			var maxX = float.MinValue;
			var minX = float.MaxValue;
			var maxY = float.MinValue;
			var minY = float.MaxValue;

			if (_useStartEndWidths)
				_maxWidth = System.Math.Max(_startWidth, _endWidth);

			// calculate line length first and simulataneously get our min/max points for the bounds
			var lineLength = 0f;
			var halfMaxWidth = _maxWidth * 0.5f;
			_points.Buffer[0].LengthFromPreviousPoint = 0;
			for (var i = 0; i < _points.Length - 1; i++)
			{
				var distance = Vector2.Distance(_points.Buffer[i].Position, _points.Buffer[i + 1].Position);
				_points.Buffer[i + 1].LengthFromPreviousPoint = distance;
				lineLength += distance;

				maxX = Mathf.MaxOf(maxX, _points.Buffer[i].Position.X + halfMaxWidth,
					_points.Buffer[i + 1].Position.X + halfMaxWidth);
				minX = Mathf.MinOf(minX, _points.Buffer[i].Position.X - halfMaxWidth,
					_points.Buffer[i + 1].Position.X - halfMaxWidth);
				maxY = Mathf.MaxOf(maxY, _points.Buffer[i].Position.Y + halfMaxWidth,
					_points.Buffer[i + 1].Position.Y + halfMaxWidth);
				minY = Mathf.MinOf(minY, _points.Buffer[i].Position.Y - halfMaxWidth,
					_points.Buffer[i + 1].Position.Y - halfMaxWidth);
			}

			_bounds.X = minX;
			_bounds.Y = minY;
			_bounds.Width = maxX - minX;
			_bounds.Height = maxY - minY;

			// special case: single segment
			if (_points.Length == 2)
			{
				if (_useStartEndWidths)
				{
					_points.Buffer[0].Width = _startWidth;
					_points.Buffer[1].Width = _endWidth;
				}

				if (_useStartEndColors)
				{
					_points.Buffer[0].Color = _startColor;
					_points.Buffer[1].Color = _endColor;
				}

				_firstSegment.SetPoints(ref _points.Buffer[0], ref _points.Buffer[1]);
				AddSingleSegmentLine(ref _firstSegment, _points.Buffer[1].Color);
				return;
			}

			var distanceSoFar = 0f;
			var fusedPoint = Vector2.Zero;
			var vertIndex = 0;
			var thirdPoint = new SegmentPoint();

			for (var i = 0; i < _points.Length - 1; i++)
			{
				var firstPoint = _points.Buffer[i];
				var secondPoint = _points.Buffer[i + 1];

				var hasThirdPoint = _points.Length > i + 2;
				if (hasThirdPoint)
					thirdPoint = _points.Buffer[i + 2];

				// we need the distance along the line of both the first and second points. distanceSoFar will always be for the furthest point
				// which is the previous point before adding the current segment distance.
				var firstPointDistance = distanceSoFar;
				distanceSoFar += secondPoint.LengthFromPreviousPoint;

				var firstPointRatio = firstPointDistance / lineLength;
				var secondPointRatio = distanceSoFar / lineLength;
				var thirdPointRatio = 0f;
				if (hasThirdPoint)
					thirdPointRatio = (distanceSoFar + thirdPoint.LengthFromPreviousPoint) / lineLength;

				if (_useStartEndColors)
				{
					ColorExt.Lerp(ref _startColor, ref _endColor, out firstPoint.Color, firstPointRatio);
					ColorExt.Lerp(ref _startColor, ref _endColor, out secondPoint.Color, secondPointRatio);

					if (hasThirdPoint)
						ColorExt.Lerp(ref _startColor, ref _endColor, out thirdPoint.Color, thirdPointRatio);
				}

				if (_useStartEndWidths)
				{
					firstPoint.Width = Mathf.Lerp(_startWidth, _endWidth, firstPointRatio);
					secondPoint.Width = Mathf.Lerp(_startWidth, _endWidth, secondPointRatio);

					if (hasThirdPoint)
						thirdPoint.Width = Mathf.Lerp(_startWidth, _endWidth, thirdPointRatio);
				}


				if (i == 0)
				{
					_firstSegment.SetPoints(ref firstPoint, ref secondPoint);
					_secondSegment.SetPoints(ref secondPoint, ref thirdPoint);
				}
				else
				{
					Utils.Swap(ref _firstSegment, ref _secondSegment);
					if (hasThirdPoint)
						_secondSegment.SetPoints(ref secondPoint, ref thirdPoint);
				}

				// dont recalculate the fusedPoint for the last segment since there will be no third point to work with
				if (hasThirdPoint)
				{
					var shouldFuseBottom =
						Vector2Ext.IsTriangleCCW(firstPoint.Position, secondPoint.Position, thirdPoint.Position);
					_secondSegment.SetFusedData(shouldFuseBottom, ref _firstSegment);
				}

				// special care needs to be take with the first segment since it has a different vert count
				if (i == 0)
					AddFirstSegment(ref _firstSegment, ref _secondSegment, ref vertIndex);
				else
					AddSegment(ref _firstSegment, ref vertIndex);

				_lastSegment.CloneFrom(ref _firstSegment);
			}
		}


		/// <summary>
		/// special case for just 2 points, one line segment
		/// </summary>
		/// <param name="segment">Segment.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddSingleSegmentLine(ref Segment segment, Color nextPointColor)
		{
			_indices.Add(0);
			_indices.Add(1);
			_indices.Add(2);

			_indices.Add(0);
			_indices.Add(2);
			_indices.Add(3);

			AddVert(0, segment.Tl, new Vector2(0, 1), _useStartEndColors ? _startColor : segment.Point.Color);
			AddVert(1, segment.Tr, new Vector2(1, 1), nextPointColor);
			AddVert(2, segment.Br, new Vector2(1, 0), nextPointColor);
			AddVert(3, segment.Bl, new Vector2(0, 0), _useStartEndColors ? _startColor : segment.Point.Color);
		}


		/// <summary>
		/// the first segment is special since it has no previous verts to connect to so we handle it separately.
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="nextSegment">Next segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddFirstSegment(ref Segment segment, ref Segment nextSegment, ref int vertIndex)
		{
			_indices.Add(0);
			_indices.Add(1);
			_indices.Add(4);

			_indices.Add(1);
			_indices.Add(2);
			_indices.Add(4);

			_indices.Add(2);
			_indices.Add(3);
			_indices.Add(4);

			// the tl vert will always be present, as weill the bl
			AddVert(vertIndex++, segment.Tl, new Vector2(0, 1), segment.Point.Color);

			if (nextSegment.ShouldFuseBottom)
			{
				AddVert(vertIndex++, segment.Tr, new Vector2(1, 1), segment.NextPoint.Color);
				AddVert(vertIndex++, nextSegment.Point.Position, new Vector2(1, 0.5f), segment.NextPoint.Color);
				AddVert(vertIndex++, nextSegment.HasFusedPoint ? nextSegment.FusedPoint : segment.Tl, new Vector2(1, 0),
					segment.NextPoint.Color);
			}
			else
			{
				AddVert(vertIndex++, nextSegment.HasFusedPoint ? nextSegment.FusedPoint : segment.Bl, new Vector2(1, 1),
					segment.NextPoint.Color);
				AddVert(vertIndex++, nextSegment.Point.Position, new Vector2(1, 0.5f), segment.NextPoint.Color);
				AddVert(vertIndex++, segment.Br, new Vector2(1, 0), segment.NextPoint.Color);
			}

			AddVert(vertIndex++, segment.Bl, new Vector2(0, 0), segment.Point.Color);
		}


		/// <summary>
		/// adds a segment and takes care of patching the previous elbow
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="vertIndex">Vert index.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddSegment(ref Segment segment, ref int vertIndex)
		{
			// first, we need to patch the previous elbow gap
			PatchJoint(ref segment, ref vertIndex);

			_indices.Add((short) vertIndex);
			_indices.Add((short) (vertIndex + 1));
			_indices.Add((short) (vertIndex + 2));

			_indices.Add((short) (vertIndex + 4));
			_indices.Add((short) vertIndex);
			_indices.Add((short) (vertIndex + 2));

			_indices.Add((short) (vertIndex + 3));
			_indices.Add((short) (vertIndex + 4));
			_indices.Add((short) (vertIndex + 2));

			if (segment.ShouldFuseBottom)
			{
				AddVert(vertIndex++, segment.Tl, new Vector2(0, 1), segment.Point.Color);
				AddVert(vertIndex++, segment.Tr, new Vector2(1, 1), segment.NextPoint.Color);
				AddVert(vertIndex++, segment.Br, new Vector2(1, 0), segment.NextPoint.Color);
				AddVert(vertIndex++, segment.HasFusedPoint ? segment.FusedPoint : segment.Bl, new Vector2(0, 0),
					segment.Point.Color);
			}
			else
			{
				AddVert(vertIndex++, segment.HasFusedPoint ? segment.FusedPoint : segment.Tl, new Vector2(0, 1),
					segment.Point.Color);
				AddVert(vertIndex++, segment.Tr, new Vector2(1, 1), segment.NextPoint.Color);
				AddVert(vertIndex++, segment.Br, new Vector2(1, 0), segment.NextPoint.Color);
				AddVert(vertIndex++, segment.Bl, new Vector2(0, 0), segment.Point.Color);
			}

			AddVert(vertIndex++, segment.Point.Position, new Vector2(1, 0.5f), segment.Point.Color);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void PatchJoint(ref Segment segment, ref int vertIndex)
		{
			switch (EndCapType)
			{
				case EndCapType.Standard:
					PatchStandardJoint(ref segment, ref vertIndex);
					break;
				case EndCapType.Jagged:
					PatchJaggedJoint(ref segment, ref vertIndex);
					break;
				case EndCapType.JaggedWithCutoff:
					if (segment.Angle < CutoffAngleForEndCapSubdivision)
						PatchJaggedJoint(ref segment, ref vertIndex);
					else
						PatchStandardJoint(ref segment, ref vertIndex);
					break;
				case EndCapType.Smooth:
					PatchSmoothJoint(ref segment, ref vertIndex);
					break;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void PatchStandardJoint(ref Segment segment, ref int vertIndex)
		{
			if (segment.ShouldFuseBottom)
			{
				_indices.Add((short) vertIndex);
				_indices.Add((short) (vertIndex + 4));
				_indices.Add((short) (vertIndex - 4));
			}
			else
			{
				// If this is the second segment we need a different vert from the first segment since the first segment has 1 less vert than
				// all mid segments.
				var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
				_indices.Add((short) (vertIndex - 3 + firstSegmentOffset));
				_indices.Add((short) (vertIndex + 4));
				_indices.Add((short) (vertIndex + 3));
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void PatchJaggedJoint(ref Segment segment, ref int vertIndex)
		{
			Vector2 intersection;
			if (segment.ShouldFuseBottom)
			{
				if (Vector2Ext.GetRayIntersection(segment.Tl, segment.Tr, _lastSegment.Tl, _lastSegment.Tr,
					out intersection))
				{
					AddVert(vertIndex++, intersection, new Vector2(1, 1), segment.Point.Color);

					_indices.Add((short) vertIndex);
					_indices.Add((short) (vertIndex + 4));
					_indices.Add((short) (vertIndex - 1));

					_indices.Add((short) (vertIndex - 1));
					_indices.Add((short) (vertIndex + 4));
					_indices.Add((short) (vertIndex - 5));
				}
			}
			else
			{
				if (Vector2Ext.GetRayIntersection(segment.Bl, segment.Br, _lastSegment.Bl, _lastSegment.Br,
					out intersection))
				{
					var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
					AddVert(vertIndex++, intersection, new Vector2(1, 0), segment.Point.Color);

					_indices.Add((short) (vertIndex + 4));
					_indices.Add((short) (vertIndex + 3));
					_indices.Add((short) (vertIndex - 1));

					_indices.Add((short) (vertIndex - 3 + firstSegmentOffset));
					_indices.Add((short) (vertIndex + 4));
					_indices.Add((short) (vertIndex - 1));
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void PatchSmoothJoint(ref Segment segment, ref int vertIndex)
		{
			if (segment.ShouldFuseBottom)
			{
				// first, we need to get the angle from the point to the tr and tl verts
				var a = _lastSegment.Tr;
				var b = segment.Tl;
				var center = segment.Point.Position;

				// we get the angle from 3 o'clock to each of the points, then get the angle in degrees of the pacman shape
				var angle1 = Mathf.Atan2(a.Y - center.Y, a.X - center.X) * Mathf.Rad2Deg;
				var angle2 = Mathf.Atan2(b.Y - center.Y, b.X - center.X) * Mathf.Rad2Deg;
				var deltaAngle = Mathf.DeltaAngle(angle1, angle2);

				// figure out how many verts we are going to add to the joint
				var totalNewVerts = Mathf.Ceil(System.Math.Abs(deltaAngle) / DegreesPerSubdivision);
				var angleIncrement = deltaAngle / (totalNewVerts + 1);

				// first triangle will go from the tr vert of the last segment, to the point, to the first new vert
				_indices.Add((short) (vertIndex)); // first new vert
				_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // point
				_indices.Add((short) (vertIndex - 4)); // tr of previous

				// add all the triangles that are not connected to either of the two segments
				for (var i = 0; i < totalNewVerts - 1; i++)
				{
					_indices.Add((short) (vertIndex + i)); // prev new vert
					_indices.Add((short) (vertIndex + i + 1)); // next new vert
					_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // point
				}

				// finally, add the last triangle
				_indices.Add((short) (vertIndex + totalNewVerts)); // 0	tl of next
				_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // point
				_indices.Add((short) (vertIndex + totalNewVerts - 1)); // last new vert

				// and now we add all the verts using the angleIncrement we calcualted earlier to step from angle1 to angle2
				for (var i = 0; i < totalNewVerts; i++)
				{
					var midAngle = angle1 + angleIncrement * (i + 1);
					var midPoint = Mathf.PointOnCircle(center, segment.Point.Width / 2, midAngle);
					AddVert(vertIndex++, midPoint, new Vector2(1, 1), segment.Point.Color);
				}
			}
			else
			{
				var a = _lastSegment.Br;
				var b = segment.Bl;
				var center = segment.Point.Position;

				var angle1 = Mathf.Atan2(a.Y - center.Y, a.X - center.X) * Mathf.Rad2Deg;
				var angle2 = Mathf.Atan2(b.Y - center.Y, b.X - center.X) * Mathf.Rad2Deg;
				var deltaAngle = Mathf.DeltaAngle(angle1, angle2);

				var totalNewVerts = Mathf.Ceil(System.Math.Abs(deltaAngle) / DegreesPerSubdivision);
				var angleIncrement = deltaAngle / (totalNewVerts + 1);

				var firstSegmentOffset = vertIndex == 5 ? 1 : 0;
				_indices.Add((short) (vertIndex - 3 + firstSegmentOffset)); // bl of previous
				_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // center
				_indices.Add((short) (vertIndex)); // first new vert

				for (var i = 0; i < totalNewVerts - 1; i++)
				{
					_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // point
					_indices.Add((short) (vertIndex + i + 1)); // next new vert
					_indices.Add((short) (vertIndex + i)); // prev new vert
				}

				_indices.Add((short) (vertIndex + 4 + totalNewVerts)); // point
				_indices.Add((short) (vertIndex + 3 + totalNewVerts)); // br of next
				_indices.Add((short) (vertIndex + totalNewVerts - 1)); // last new vert

				for (var i = 0; i < totalNewVerts; i++)
				{
					var midAngle = angle1 + angleIncrement * (i + 1);
					var midPoint = Mathf.PointOnCircle(center, segment.Point.Width / 2, midAngle);
					AddVert(vertIndex++, midPoint, new Vector2(1, 0), segment.Point.Color);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddVert(int index, Vector2 position, Vector2 texCoord, Color col)
		{
			_vertices.EnsureCapacity();
			_vertices.Buffer[index].Position = position.ToVector3();
			_vertices.Buffer[index].TextureCoordinate = texCoord;
			_vertices.Buffer[index].Color = col;
			_vertices.Length++;
		}


		#region Component/RenderableComponent

		public override void OnAddedToEntity()
		{
			_basicEffect = Entity.Scene.Content.LoadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;

			if (_texture != null)
			{
				_basicEffect.Texture = _texture;
				_basicEffect.TextureEnabled = true;
				_texture = null;
			}
		}


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			// we dont care if the transform changed if we are in world space
			if (UseWorldSpace)
				return;

			_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, Vector2.Zero, Entity.Transform.Scale,
				Entity.Transform.Rotation, Width, Height);
		}


		public override bool IsVisibleFromCamera(Camera camera)
		{
			CalculateVertices();
			return base.IsVisibleFromCamera(camera);
		}


		public override void Render(Batcher batcher, Camera camera)
		{
			if (_points.Length < 2)
				return;
			
			batcher.FlushBatch();

			_basicEffect.Projection = camera.ProjectionMatrix;
			_basicEffect.View = camera.TransformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			if (!UseWorldSpace)
				_basicEffect.World = Transform.LocalToWorldTransform;

			var primitiveCount = _indices.Length / 3;
			Core.GraphicsDevice.SamplerStates[0] = Core.DefaultWrappedSamplerState;
			Core.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices.Buffer, 0,
				_vertices.Length, _indices.Buffer, 0, primitiveCount);
		}


		public override void DebugRender(Batcher batcher)
		{
			for (var i = 0; i < _vertices.Length; i++)
			{
				var v = _vertices[i];
				batcher.DrawPixel(v.Position.X, v.Position.Y, Color.GhostWhite, 4);
			}

			batcher.DrawHollowRect(_bounds, Debug.Colors.ColliderBounds);
		}

		#endregion


		#region Helper classes

		struct SegmentPoint
		{
			public Vector2 Position;
			public Color Color;
			public float Width;
			public float LengthFromPreviousPoint;
		}


		/// <summary>
		/// helper class used to store some data when calculating verts
		/// </summary>
		class Segment
		{
			public Vector2 Tl, Tr, Br, Bl;
			public SegmentPoint Point;
			public SegmentPoint NextPoint;
			public Vector2 FusedPoint;
			public bool HasFusedPoint;
			public bool ShouldFuseBottom;
			public float Angle;


			public void SetPoints(ref SegmentPoint point, ref SegmentPoint nextPoint)
			{
				Angle = 0;
				Point = point;
				NextPoint = nextPoint;

				// rotate 90 degrees before calculating and cache cos/sin
				var radians = Mathf.Atan2(nextPoint.Position.Y - point.Position.Y,
					nextPoint.Position.X - point.Position.X);
				radians += MathHelper.PiOver2;
				var halfCos = Mathf.Cos(radians) * 0.5f;
				var halfSin = Mathf.Sin(radians) * 0.5f;

				Tl = point.Position - new Vector2(point.Width * halfCos, point.Width * halfSin);
				Tr = nextPoint.Position - new Vector2(nextPoint.Width * halfCos, nextPoint.Width * halfSin);
				Br = nextPoint.Position + new Vector2(nextPoint.Width * halfCos, nextPoint.Width * halfSin);
				Bl = point.Position + new Vector2(point.Width * halfCos, point.Width * halfSin);
			}


			public void SetFusedData(bool shouldFuseBottom, ref Segment segment)
			{
				// store the angle off for later. For extreme angles we add extra verts to smooth the joint
				Angle = Vector2Ext.Angle(segment.Point.Position - Point.Position, NextPoint.Position - Point.Position);
				ShouldFuseBottom = shouldFuseBottom;

				if (shouldFuseBottom)
					HasFusedPoint = ShapeCollisions.LineToLine(segment.Bl, segment.Br, Bl, Br, out FusedPoint);
				else
					HasFusedPoint = ShapeCollisions.LineToLine(segment.Tl, segment.Tr, Tl, Tr, out FusedPoint);
			}


			public void CloneFrom(ref Segment segment)
			{
				Tl = segment.Tl;
				Tr = segment.Tr;
				Br = segment.Br;
				Bl = segment.Bl;
				Point = segment.Point;
				NextPoint = segment.NextPoint;
				HasFusedPoint = segment.HasFusedPoint;
				ShouldFuseBottom = segment.ShouldFuseBottom;
				Angle = segment.Angle;
			}
		}

		#endregion
	}
}
