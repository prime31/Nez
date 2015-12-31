using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		public Vector2[] points;


		public Polygon( Vector2[] points )
		{
			this.points = points;
		}


		public Polygon( int vertCount, float radius ) : this( buildSymmetricalPolygon( vertCount, radius ) )
		{}


		internal override void recalculateBounds( Collider collider )
		{
			bounds = RectangleExt.boundsFromPolygonPoints( points );
			bounds.Location += collider.absolutePosition.ToPoint();
		}


		static Vector2[] buildSymmetricalPolygon( int vertCount, float radius )
		{
			var verts = new Vector2[vertCount];

			for( var i = 0; i < vertCount; i++ )
			{
				var a = 2.0f * MathHelper.Pi * ( i / (float)vertCount );
				verts[i] = new Vector2( Mathf.cos( a ), Mathf.sin( a ) ) * radius;
			}

			return verts;
		}


		public override bool collidesWithShape( Shape other, Vector2 deltaMovement, out ShapeCollisionResult result )
		{
			result = new ShapeCollisionResult();

			if( other is Polygon )
				return ShapeCollisions.polygonToPolygon( this, other as Polygon, out result );

			if( other is Circle && ShapeCollisions.circleToPolygon( other as Circle, this, out result ) )
			{
				// TODO: flip the result since the colliding objects are reversed
				return true;
			}

			return false;
		}


		public override bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit )
		{
			hit = new RaycastHit();
			return ShapeCollisions.lineToPoly( start, end, this, out hit );
		}

	}
}

