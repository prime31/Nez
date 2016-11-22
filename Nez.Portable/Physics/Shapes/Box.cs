using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	/// <summary>
	/// special case of a Polygon. When doing SAT collision checks we only need to check 2 axes instead of 8
	/// </summary>
	public class Box : Polygon
	{
		public float width;
		public float height;


		public Box( float width, float height ) : base( buildBox( width, height ), true )
		{
			isBox = true;
			this.width = width;
			this.height = height;
		}


		/// <summary>
		/// helper that builds the points a Polygon needs in the shape of a box
		/// </summary>
		/// <returns>The box.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		static Vector2[] buildBox( float width, float height )
		{
			// we create our points around a center of 0,0
			var halfWidth = width / 2;
			var halfHeight = height / 2;
			var verts = new Vector2[4];

			verts[0] = new Vector2( -halfWidth, -halfHeight );
			verts[1] = new Vector2( halfWidth, -halfHeight );
			verts[2] = new Vector2( halfWidth, halfHeight );
			verts[3] = new Vector2( -halfWidth, halfHeight );

			return verts;
		}


		/// <summary>
		/// updates the Box points, recalculates the center and sets the width/height
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void updateBox( float width, float height )
		{
			this.width = width;
			this.height = height;

			// we create our points around a center of 0,0
			var halfWidth = width / 2;
			var halfHeight = height / 2;

			points[0] = new Vector2( -halfWidth, -halfHeight );
			points[1] = new Vector2( halfWidth, -halfHeight );
			points[2] = new Vector2( halfWidth, halfHeight );
			points[3] = new Vector2( -halfWidth, halfHeight );

			for( var i = 0; i < points.Length; i++ )
				_originalPoints[i] = points[i];
		}


		#region Shape abstract methods

		public override bool overlaps( Shape other )
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if( isUnrotated )
			{
				if( other is Box && ( other as Box ).isUnrotated )
					return bounds.intersects( ref ( other as Box ).bounds );

				if( other is Circle )
					return Collisions.rectToCircle( ref bounds, other.position, ( other as Circle ).radius );
			}

			// fallthrough to standard cases
			return base.overlaps( other );
		}


		public override bool collidesWithShape( Shape other, out CollisionResult result )
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if( isUnrotated && other is Box && ( other as Box ).isUnrotated )
				return ShapeCollisions.boxToBox( this, other as Box, out result );

			// TODO: get Minkowski working for circle to box
			//if( other is Circle )

			// fallthrough to standard cases
			return base.collidesWithShape( other, out result );
		}


		public override bool containsPoint( Vector2 point )
		{
			if( isUnrotated )
				return bounds.contains( point );
			
			return base.containsPoint( point );
		}


		public override bool pointCollidesWithShape( Vector2 point, out CollisionResult result )
		{
			if( isUnrotated )
				return ShapeCollisions.pointToBox( point, this, out result );

			return base.pointCollidesWithShape( point, out result );
		}

		#endregion

	}
}

