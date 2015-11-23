using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Collisions
	{
		[Flags]
		public enum PointSectors
		{
			Center = 0,
			Top = 1,
			Bottom = 2,
			TopLeft = 9,
			TopRight = 5,
			Left = 8,
			Right = 4,
			BottomLeft = 10,
			BottomRight = 6
		};



		#region Line

		static public bool lineCheck( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2 )
		{
			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			Vector2 c = b1 - a1;
			float t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			float u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			return true;
		}


		static public bool lineCheck( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection )
		{
			intersection = Vector2.Zero;

			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			Vector2 c = b1 - a1;
			float t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			float u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			intersection = a1 + t * b;

			return true;
		}

		#endregion


		#region Circle

		static public Vector2 closestPointOnLine( Vector2 lineA, Vector2 lineB, Vector2 closestTo )
		{
			var v = lineB - lineA;
			var w = closestTo - lineA;
			var t = Vector2.Dot( w, v ) / Vector2.Dot( v, v );
			t = MathHelper.Clamp( t, 0, 1 );

			return lineA + v * t;
		}


		static public bool circleToLine( Vector2 cPosiition, float cRadius, Vector2 lineFrom, Vector2 lineTo )
		{
			return Vector2.DistanceSquared( cPosiition, closestPointOnLine( lineFrom, lineTo, cPosiition ) ) < cRadius * cRadius;
		}


		static public bool circleToPoint( Vector2 cPosition, float cRadius, Vector2 point )
		{
			return Vector2.DistanceSquared( cPosition, point ) < cRadius * cRadius;
		}

		#endregion


		#region Bounds/Rect

		static public bool rectToCircle( float rX, float rY, float rW, float rH, Vector2 cPosition, float cRadius )
		{
			//Check if the circle contains the rectangle's center-point
			if( Collisions.circleToPoint( cPosition, cRadius, new Vector2( rX + rW / 2, rY + rH / 2 ) ) )
				return true;

			//Check the circle against the relevant edges
			Vector2 edgeFrom;
			Vector2 edgeTo;
			var sector = getSector( rX, rY, rW, rH, cPosition );

			if( ( sector & PointSectors.Top ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY );
				edgeTo = new Vector2( rX + rW, rY );
				if( circleToLine( cPosition, cRadius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Bottom ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY + rH );
				edgeTo = new Vector2( rX + rW, rY + rH );
				if( circleToLine( cPosition, cRadius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Left ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY );
				edgeTo = new Vector2( rX, rY + rH );
				if( circleToLine( cPosition, cRadius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Right ) != 0 )
			{
				edgeFrom = new Vector2( rX + rW, rY );
				edgeTo = new Vector2( rX + rW, rY + rH );
				if( circleToLine( cPosition, cRadius, edgeFrom, edgeTo ) )
					return true;
			}

			return false;
		}


		static public bool rectToCircle( Rectangle rect, Vector2 cPosition, float cRadius )
		{
			return rectToCircle( rect.X, rect.Y, rect.Width, rect.Height, cPosition, cRadius );
		}


		static public bool boundsToCircle( Rectangle bounds, Vector2 cPosition, float cRadius )
		{
			return rectToCircle( bounds.X, bounds.Y, bounds.Width, bounds.Height, cPosition, cRadius );
		}


		static public bool rectToLine( float rX, float rY, float rW, float rH, Vector2 lineFrom, Vector2 lineTo )
		{
			var fromSector = Collisions.getSector( rX, rY, rW, rH, lineFrom );
			var toSector = Collisions.getSector( rX, rY, rW, rH, lineTo );

			if( fromSector == PointSectors.Center || toSector == PointSectors.Center )
				return true;
			else if( ( fromSector & toSector ) != 0 )
				return false;
			else
			{
				PointSectors both = fromSector | toSector;

				//Do line checks against the edges
				Vector2 edgeFrom;
				Vector2 edgeTo;

				if( ( both & PointSectors.Top ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY );
					edgeTo = new Vector2( rX + rW, rY );
					if( Collisions.lineCheck( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Bottom ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY + rH );
					edgeTo = new Vector2( rX + rW, rY + rH );
					if( Collisions.lineCheck( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Left ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY );
					edgeTo = new Vector2( rX, rY + rH );
					if( Collisions.lineCheck( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Right ) != 0 )
				{
					edgeFrom = new Vector2( rX + rW, rY );
					edgeTo = new Vector2( rX + rW, rY + rH );
					if( Collisions.lineCheck( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}
			}

			return false;
		}


		static public bool rectToLine( Rectangle rect, Vector2 lineFrom, Vector2 lineTo )
		{
			return rectToLine( rect.X, rect.Y, rect.Width, rect.Height, lineFrom, lineTo );
		}


		static public bool rectToPoint( float rX, float rY, float rW, float rH, Vector2 point )
		{
			return point.X >= rX && point.Y >= rY && point.X < rX + rW && point.Y < rY + rH;
		}


		static public bool rectToPoint( Rectangle rect, Vector2 point )
		{
			return rectToPoint( rect.X, rect.Y, rect.Width, rect.Height, point );
		}

		#endregion


		#region Sectors

		/*
         *  Bitflags and helpers for using the Cohen–Sutherland algorithm
         *  http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
         *  
         *  Sector bitflags:
         *      1001  1000  1010
         *      0001  0000  0010
         *      0101  0100  0110
         */

		static public PointSectors getSector( Rectangle rect, Vector2 point )
		{
			PointSectors sector = PointSectors.Center;

			if( point.X < rect.Left )
				sector |= PointSectors.Left;
			else if( point.X >= rect.Right )
				sector |= PointSectors.Right;

			if( point.Y < rect.Top )
				sector |= PointSectors.Top;
			else if( point.Y >= rect.Bottom )
				sector |= PointSectors.Bottom;

			return sector;
		}


		static public PointSectors getSector( float rX, float rY, float rW, float rH, Vector2 point )
		{
			PointSectors sector = PointSectors.Center;

			if( point.X < rX )
				sector |= PointSectors.Left;
			else if( point.X >= rX + rW )
				sector |= PointSectors.Right;

			if( point.Y < rY )
				sector |= PointSectors.Top;
			else if( point.Y >= rY + rH )
				sector |= PointSectors.Bottom;

			return sector;
		}

		#endregion
	
	}
}

