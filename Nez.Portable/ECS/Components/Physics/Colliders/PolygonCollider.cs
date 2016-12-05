using System;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	/// <summary>
	/// Polygons should be defined in clockwise fashion.
	/// </summary>
	public class PolygonCollider : Collider
	{
		/// <summary>
		/// If the points are not centered they will be centered with the difference being applied to the localOffset.
		/// </summary>
		/// <param name="points">Points.</param>
		public PolygonCollider( Vector2[] points )
		{
			// first and last point must not be the same. we want an open polygon
			var isPolygonClosed = points[0] == points[points.Length - 1];

			if( isPolygonClosed )
				Array.Resize( ref points, points.Length - 1 );

			var center = Polygon.findPolygonCenter( points );
			setLocalOffset( center );
			Polygon.recenterPolygonVerts( points );
			shape = new Polygon( points );
		}


		public PolygonCollider( int vertCount, float radius )
		{
			shape = new Polygon( vertCount, radius );
		}


		public override void debugRender( Graphics graphics )
		{
			var poly = shape as Polygon;
			graphics.batcher.drawHollowRect( bounds, Debug.Colors.colliderBounds, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPolygon( shape.position, poly.points, Debug.Colors.colliderEdge, true, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( entity.transform.position, Debug.Colors.colliderPosition, 4 * Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( shape.position, Debug.Colors.colliderCenter, 2 * Debug.Size.lineSizeMultiplier );

			// Normal debug code
			//for( var i = 0; i < poly.points.Length; i++ )
			//{
			//	Vector2 p2;
			//	var p1 = poly.points[i];
			//	if( i + 1 >= poly.points.Length )
			//		p2 = poly.points[0];
			//	else
			//		p2 = poly.points[i + 1];
			//	var perp = Vector2Ext.perpendicular( ref p1, ref p2 );
			//	Vector2Ext.normalize( ref perp );
			//	var mp = Vector2.Lerp( p1, p2, 0.5f ) + poly.position;
			//	graphics.batcher.drawLine( mp, mp + perp * 10, Color.White );
			//}
		}

	}
}

