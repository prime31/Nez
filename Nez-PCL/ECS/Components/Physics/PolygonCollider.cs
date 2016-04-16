using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
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

			// create the array with an extra element if we need to close the poly
			var tempPoints = new Vector2[ isPolygonClosed ? points.Length - 1 : points.Length];

			// copy our points over
			for( var i = 0; i < tempPoints.Length; i++ )
				tempPoints[i] = points[i];

			shape = new Polygon( tempPoints );
		}


		public PolygonCollider( int vertCount, float radius )
		{
			shape = new Polygon( vertCount, radius );
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawHollowRect( shape.bounds, Color.Black );
			graphics.batcher.drawPolygon( absolutePosition, ((shape as Polygon).points), Color.DarkRed, true );
			graphics.batcher.drawPixel( absolutePosition, Color.Yellow, 4 );
		}

	}
}

