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
		public PolygonCollider( Vector2[] points )
		{
			// first and last point must not be the same. we want an open polygon
			var isPolygonClosed = points[0] == points[points.Length - 1];

			if( isPolygonClosed )
				Array.Resize( ref points, points.Length - 1 );

			shape = new Polygon( points );
		}


		public PolygonCollider( int vertCount, float radius )
		{
			shape = new Polygon( vertCount, radius );
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawHollowRect( shape.bounds, Color.White * 0.3f );
			graphics.batcher.drawPolygon( absolutePosition, ( ( shape as Polygon ).points ), Color.DarkRed, true );
			graphics.batcher.drawPixel( absolutePosition, Color.Yellow, 4 );
			graphics.batcher.drawPixel( ( shape as Polygon ).center + absolutePosition, Color.Red, 4 );
		}

	}
}

