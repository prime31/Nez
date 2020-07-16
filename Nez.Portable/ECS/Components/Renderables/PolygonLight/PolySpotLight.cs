using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Shadows
{
	/// <summary>
	/// WIP: still has some odd rendering bugs that need to get worked out
	/// poly spot light. Works just like a PolyLight except it is limited to a cone shape (spotAngle).
	/// </summary>
	public class PolySpotLight : PolyLight
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds = RectangleF.RectEncompassingPoints(_polygon.Points);
					_bounds.Location += Entity.Transform.Position;
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// the angle of the light's spotlight cone in degrees. Defaults to 45.
		/// </summary>
		/// <value>The spot angle.</value>
		[Range(0, 360)]
		public float SpotAngle
		{
			get => _spotAngle;
			set => SetSpotAngle(value);
		}

		float _spotAngle = 45;
		Polygon _polygon;


		public PolySpotLight() : this(400)
		{
		}

		public PolySpotLight(float radius) : this(radius, Color.White)
		{
		}

		public PolySpotLight(float radius, Color color) : this(radius, color, 1.0f)
		{
		}

		public PolySpotLight(float radius, Color color, float power) : base(radius, color, power)
		{
		}


		#region Fluent setters

		public override PolyLight SetRadius(float radius)
		{
			base.SetRadius(radius);
			RecalculatePolyPoints();
			return this;
		}

		public PolySpotLight SetSpotAngle(float spotAngle)
		{
			_spotAngle = spotAngle;
			RecalculatePolyPoints();
			return this;
		}

		#endregion


		/// <summary>
		/// calculates the points needed to encompass the spot light. The points generate a polygon which is used for overlap detection.
		/// </summary>
		void RecalculatePolyPoints()
		{
			// no need to recaluc if we dont have an Entity to work with
			if (Entity == null)
				return;

			// we only need a small number of verts for the spot polygon. We base how many off of the spot angle. Because we are approximating
			// an arc with a polygon we bump up the radius a bit so that our poly fully encompasses the spot area.
			var expandedRadius = Radius + Radius * 0.1f;
			var sides = Mathf.CeilToInt(_spotAngle / 25) + 1;
			var stepSize = (_spotAngle * Mathf.Deg2Rad) / sides;

			var verts = new Vector2[sides + 2];
			verts[0] = Vector2.Zero;

			for (var i = 0; i <= sides; i++)
				verts[i + 1] = new Vector2(expandedRadius * Mathf.Cos(stepSize * i),
					expandedRadius * Mathf.Sin(stepSize * i));

			if (_polygon == null)
				_polygon = new Polygon(verts);
			else if (_polygon._originalPoints.Length == verts.Length)
				_polygon._originalPoints = verts;
			else
				_polygon.SetPoints(verts);

			// rotate our verts based on the Entity.rotation and offset half of the spot angle so that the center of the spot points in
			// the direction of rotation
			Polygon.RotatePolygonVerts(Entity.Rotation - _spotAngle * 0.5f * Mathf.Deg2Rad, _polygon._originalPoints,
				_polygon.Points);
		}


		#region Component overrides

		public override void OnAddedToEntity()
		{
			base.OnAddedToEntity();
			RecalculatePolyPoints();
		}

		public override void DebugRender(Batcher batcher)
		{
			base.DebugRender(batcher);
			batcher.DrawPolygon(_polygon.position, _polygon.Points, Debug.Colors.ColliderEdge, true,
				Debug.Size.LineSizeMultiplier);
		}

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			base.OnEntityTransformChanged(comp);

			if (comp == Transform.Component.Rotation)
			{
				// when rotation changes we need to update our verts to account for the new rotation
				Polygon.RotatePolygonVerts(Entity.Rotation - _spotAngle * 0.5f * Mathf.Deg2Rad,
					_polygon._originalPoints, _polygon.Points);
			}
		}

		#endregion


		#region PolyLight overrides

		protected override int GetOverlappedColliders()
		{
			CollisionResult result;
			var totalCollisions = 0;
			_polygon.position = Entity.Transform.Position + _localOffset;

			var neighbors = Physics.BoxcastBroadphase(Bounds, CollidesWithLayers);
			foreach (var neighbor in neighbors)
			{
				// skip triggers
				if (neighbor.IsTrigger)
					continue;

				if (_polygon.CollidesWithShape(neighbor.Shape, out result))
				{
					_colliderCache[totalCollisions++] = neighbor;
					if (totalCollisions == _colliderCache.Length)
						return totalCollisions;
				}
			}

			return totalCollisions;
		}

		protected override void LoadVisibilityBoundaries()
		{
			_visibility.LoadSpotLightBoundaries(_polygon.Points);
		}

		#endregion
	}
}