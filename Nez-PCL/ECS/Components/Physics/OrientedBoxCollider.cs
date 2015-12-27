using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	// http://www.flipcode.com/archives/2D_OBB_Intersection.shtml
	/// <summary>
	/// Collisions.orientedBoxColliderOverlapsOneWay performs the real work. It tests to see whether the boxes overlap.
	/// To make the tests extremely efficient, OBB._corner0ProjectionsOnEdges stores the projection
	/// of corner number zero onto a box's axes and the axes are stored explicitly in OBB.edges. The magnitude of these stored
	/// axes is the inverse of the corresponding edge length so that all overlap tests can be performed on the interval [0, 1]
	/// without normalization, and square roots are avoided throughout the entire class.
	/// </summary>
	public class OrientedBoxCollider : PolygonCollider
	{
		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					var minX = (int)Mathf.minOf( worldSpacePoints[0].X, worldSpacePoints[1].X, worldSpacePoints[2].X, worldSpacePoints[3].X );
					var maxX = (int)Mathf.maxOf( worldSpacePoints[0].X, worldSpacePoints[1].X, worldSpacePoints[2].X, worldSpacePoints[3].X );
					var minY = (int)Mathf.minOf( worldSpacePoints[0].Y, worldSpacePoints[1].Y, worldSpacePoints[2].Y, worldSpacePoints[3].Y );
					var maxY = (int)Mathf.maxOf( worldSpacePoints[0].Y, worldSpacePoints[1].Y, worldSpacePoints[2].Y, worldSpacePoints[3].Y );

					_bounds.Location = new Point( minX, minY );
					_bounds.Width = (int)( maxX - minX );
					_bounds.Height = (int)( maxY - minY );

					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// cached projection of point0 onto both edges.
		/// _corner0ProjectionsOnEdges[i] = Vector2.Dot( points[0], edges[i] );
		/// </summary>
		internal float[] _corner0ProjectionsOnEdges = new float[2];


		public OrientedBoxCollider( float width, float height )
		{
			// prep our arrays
			_points = new Vector2[4];
			edges = new Vector2[2];
			_worldSpacePoints = new Vector2[4];

			// prep our local-space points
			var x = new Vector2( width / 2, 0 );
			var y = new Vector2( 0, height / 2 );

			_points[0] = -x - y;
			_points[1] = x - y;
			_points[2] = x + y;
			_points[3] = -x + y;
		}


		/// <summary>
		/// Updates the edges after the corners move. Assumes the corners actually form a rectangle.
		/// </summary>
		protected override void buildEdges()
		{
			edges[0] = worldSpacePoints[1] - worldSpacePoints[0];
			edges[1] = worldSpacePoints[3] - worldSpacePoints[0];

			// make the length of each axis 1/edge length so we know any dot product must be less than 1 to fall within the edge.
			for( var i = 0; i < 2; ++i )
			{
				edges[i] /= edges[i].LengthSquared();
				_corner0ProjectionsOnEdges[i] = Vector2.Dot( worldSpacePoints[0], edges[i] );
			}
		}


		#region Collisions

		public override bool collidesWith( PolygonCollider polygon )
		{
			// TODO: orientedBoxColliderToOrientedBoxCollider overlaps a couple pixels sometimes
			//if( polygon is OrientedBoxCollider )
			//	return Collisions.orientedBoxColliderToOrientedBoxCollider( this, polygon as OrientedBoxCollider );

			// cant shortcut since the other one is not an OBB
			var result = Collisions.polygonToPolygon( this, polygon );
			return result.intersects;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.spriteBatch.drawPolygon( Vector2.Zero, worldSpacePoints, Color.DarkRed, true );
			graphics.spriteBatch.drawPixel( entity.position + _localPosition - _origin, Color.Yellow, 2 );
		}
	}
}

