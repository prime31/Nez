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
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleF.rectEncompassingPoints( _polygon.points );
					_bounds.location += entity.transform.position;
					_areBoundsDirty = false;
				}
				return _bounds;
			}
		}

		/// <summary>
		/// the angle of the light's spotlight cone in degrees. Defaults to 45.
		/// </summary>
		/// <value>The spot angle.</value>
		public float spotAngle
		{
			get { return _spotAngle; }
			set { setSpotAngle( value ); }
		}

		float _spotAngle = 45;
		Polygon _polygon;


		public PolySpotLight( float radius ) : this( radius, Color.White )
		{ }


		public PolySpotLight( float radius, Color color ) : this( radius, color, 1.0f )
		{ }


		public PolySpotLight( float radius, Color color, float power ) : base( radius, color, power )
		{ }


		#region Fluent setters

		public override PolyLight setRadius( float radius )
		{
			base.setRadius( radius );
			recalculatePolyPoints();
			return this;
		}


		public PolySpotLight setSpotAngle( float spotAngle )
		{
			_spotAngle = spotAngle;
			recalculatePolyPoints();
			return this;
		}

		#endregion


		/// <summary>
		/// calculates the points needed to encompass the spot light. The points generate a polygon which is used for overlap detection.
		/// </summary>
		void recalculatePolyPoints()
		{
			// no need to recaluc if we dont have an Entity to work with
			if( entity == null )
				return;

			// we only need a small number of verts for the spot polygon. We base how many off of the spot angle. Because we are approximating
			// an arc with a polygon we bump up the radius a bit so that our poly fully encompasses the spot area.
			var expandedRadius = radius + radius * 0.1f;
			var sides = Mathf.ceilToInt( _spotAngle / 25 ) + 1;
			var stepSize = ( _spotAngle * Mathf.deg2Rad ) / sides;

			var verts = new Vector2[sides + 2];
			verts[0] = Vector2.Zero;

			for( var i = 0; i <= sides; i++ )
				verts[i + 1] = new Vector2( expandedRadius * Mathf.cos( stepSize * i ), expandedRadius * Mathf.sin( stepSize * i ) );

			if( _polygon == null )
				_polygon = new Polygon( verts );
			else
				_polygon._originalPoints = verts;

			// rotate our verts based on the Entity.rotation and offset half of the spot angle so that the center of the spot points in
			// the direction of rotation
			Polygon.rotatePolygonVerts( entity.rotation - _spotAngle * 0.5f * Mathf.deg2Rad, _polygon._originalPoints, _polygon.points );
		}


		#region Component overrides

		public override void onAddedToEntity()
		{
			base.onAddedToEntity();
			recalculatePolyPoints();
		}


		public override void debugRender( Graphics graphics )
		{
			base.debugRender( graphics );
			graphics.batcher.drawPolygon( _polygon.position, _polygon.points, Debug.Colors.colliderEdge, true, Debug.Size.lineSizeMultiplier );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			base.onEntityTransformChanged( comp );

			if( comp == Transform.Component.Rotation )
			{
				// when rotation changes we need to update our verts to account for the new rotation
				Polygon.rotatePolygonVerts( entity.rotation - _spotAngle * 0.5f * Mathf.deg2Rad, _polygon._originalPoints, _polygon.points );
			}
		}

		#endregion


		#region PolyLight overrides

		protected override int getOverlappedColliders()
		{
			CollisionResult result;
			var totalCollisions = 0;
			_polygon.position = entity.transform.position + _localOffset;

			var neighbors = Physics.boxcastBroadphase( bounds, collidesWithLayers );
			foreach( var neighbor in neighbors )
			{
				// skip triggers
				if( neighbor.isTrigger )
					continue;

				if( _polygon.collidesWithShape( neighbor.shape, out result ) )
				{
					_colliderCache[totalCollisions++] = neighbor;
					if( totalCollisions == _colliderCache.Length )
						return totalCollisions;
				}
			}

			return totalCollisions;
		}


		protected override void loadVisibilityBoundaries()
		{
			_visibility.loadSpotLightBoundaries( _polygon.points );
		}

		#endregion

	}
}
