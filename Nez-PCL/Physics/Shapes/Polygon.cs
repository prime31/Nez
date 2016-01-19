using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		public Vector2[] points;
		internal bool isBox;


		public Polygon( Vector2[] points )
		{
			this.points = points;
		}


		/// <summary>
		/// creates a symmetrical polygon based on the radius and vertCount passed in
		/// </summary>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
		public Polygon( int vertCount, float radius ) : this( buildSymmetricalPolygon( vertCount, radius ) )
		{}


		public static Vector2[] buildSymmetricalPolygon( int vertCount, float radius )
		{
			var verts = new Vector2[vertCount];

			for( var i = 0; i < vertCount; i++ )
			{
				var a = 2.0f * MathHelper.Pi * ( i / (float)vertCount );
				verts[i] = new Vector2( Mathf.cos( a ), Mathf.sin( a ) ) * radius;
			}

			return verts;
		}


		internal override void recalculateBounds( Collider collider )
		{
			position = collider.absolutePosition;
			bounds = RectangleExt.boundsFromPolygonPoints( points );
			bounds.Location += position.ToPoint();
		}


		// Dont know  adjancent vertices so take each vertex
		// If you know adjancent vertices, perform hill climbing algorithm
		public Vector2 getFarthestPointInDirection( Vector2 direction )
		{
			var index = 0;
			float dot;
			float maxDot;
			Vector2.Dot( ref points[index], ref direction, out maxDot );

			for( var i = 1; i < points.Length; i++ )
			{
				Vector2.Dot( ref points[i], ref direction, out dot );

				if( dot > maxDot )
				{
					maxDot = dot;
					index = i;
				}
			}

			return points[index];
		}


		public override bool collidesWithShape( Shape other, out ShapeCollisionResult result )
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

