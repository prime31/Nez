using System;
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


		public Box( float width, float height ) : base( buildBox( width, height ) )
		{
			isBox = true;
			this.width = width;
			this.height = height;
		}


		internal override void recalculateBounds( Collider collider )
		{
			updateBox( width, height );
			base.recalculateBounds( collider );
		}


		public void updateBox( float width, float height )
		{
			points[0] = new Vector2( 0, 0 );
			points[1] = new Vector2( width, 0 );
			points[2] = new Vector2( width, height );
			points[3] = new Vector2( 0, height );
		}


		static Vector2[] buildBox( float width, float height )
		{
			var verts = new Vector2[4];

			verts[0] = new Vector2( 0, 0 );
			verts[1] = new Vector2( width, 0 );
			verts[2] = new Vector2( width, height );
			verts[3] = new Vector2( 0, height );

			return verts;
		}


		public RectangleF minkowskiDifference( Box other )
		{
			var topLeft = position - other.bounds.max;
			var fullSize = bounds.size + other.bounds.size;
			return new RectangleF( topLeft.X, topLeft.Y, fullSize.X, fullSize.Y );
		}


		public override bool overlaps( Shape other )
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if( other is Box )
				return bounds.intersects( ref ( other as Box ).bounds );

			if( other is Circle )
				return Collisions.rectToCircle( ref bounds, other.position, ( other as Circle ).radius );

			// fallthrough to standard cases
			return base.overlaps( other );
		}


		public override bool collidesWithShape( Shape other, out CollisionResult result )
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if( other is Box )
				return ShapeCollisions.boxToBox( this, other as Box, out result );

			// TODO: get Minkowski working for circle to box
			//if( other is Circle )

			// fallthrough to standard cases
			return base.collidesWithShape( other, out result );
		}

	}
}

