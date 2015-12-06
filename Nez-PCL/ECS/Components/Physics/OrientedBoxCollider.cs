using System;
using Microsoft.Xna.Framework;


/// <summary>
/// This class still needs a lot of love. It should probably technically be a PolygonCollider subclass so that it can piggyback on top of
/// all the PolygonCollider collider methods. They can then have a special branch for when we have an OrientedBoxCollider-to-OrientedBoxCollider
/// collision to hit the fast code path.
/// 
/// - rotation currently doesn't take into account the origin
/// - points are in local space but overlapsOneWay really wants them to be in world space
/// </summary>
namespace Nez.Experimental
{
	// http://www.flipcode.com/archives/2D_OBB_Intersection.shtml
	/// <summary>
	/// OBB.overlapsOneWay performs the real work. It tests to see whether the box passed as an argument overlaps the
	/// current box along either of the current box's dimensions. Note that this test must be performed for each box on the other
	/// to determine whether there is truly any overlap. To make the tests extremely efficient, OBB.origin stores the projection
	/// of corner number zero onto a box's axes and the axes are stored explicitly in OBB.edges. The magnitude of these stored
	/// axes is the inverse of the corresponding edge length so that all overlap tests can be performed on the interval [0, 1]
	/// without normalization, and square roots are avoided throughout the entire class.
	/// </summary>
	public class OrientedBoxCollider : Collider
	{
		public override float width { get { return bounds.Width; } set { throw new NotSupportedException(); } }
		public override float height { get { return bounds.Height; } set { throw new NotSupportedException(); } }

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					var minX = (int)Mathf.minOf( _points[0].X, _points[1].X, _points[2].X, _points[3].X );
					var maxX = (int)Mathf.maxOf( _points[0].X, _points[1].X, _points[2].X, _points[3].X );
					var minY = (int)Mathf.minOf( _points[0].Y, _points[1].Y, _points[2].Y, _points[3].Y );
					var maxY = (int)Mathf.maxOf( _points[0].Y, _points[1].Y, _points[2].Y, _points[3].Y );

					// add in our position/origin
					var rootPositionX = (int)( entity.position.X + _localPosition.X - _origin.X );
					var rootPositionY = (int)( entity.position.Y + _localPosition.Y - _origin.Y );
					_bounds.Location = new Point( rootPositionX + minX, rootPositionY + minY );
					_bounds.Width = (int)( maxX - minX );
					_bounds.Height = (int)( maxY - minY );

					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Corners of the box, where 0 is the top-left
		/// </summary>
		Vector2[] _points = new Vector2[4];

		/// <summary>
		/// Two edges of the box extended away from corner[0]
		/// </summary>
		Vector2[] _edges = new Vector2[2];

		/// <summary>
		/// cached projection of point0 onto both edges.
		/// _corner0ProjectionsOnEdges[i] = Vector2.Dot( points[0], edges[i] );
		/// </summary>
		float[] _corner0ProjectionsOnEdges = new float[2];


		public OrientedBoxCollider( float width, float height, float angle )
		{
			// alternative way to translate points via Matrix
			/*
			var transformMatrix = Matrix.CreateTranslation( -center.X, -center.Y, 0f ) *
				Matrix.CreateRotationZ( angle ) *
				Matrix.CreateTranslation( center.X, center.Y, 0f );

			var tl = new Vector2( center.X - w / 2f, center.Y - h / 2f );
			var tr = new Vector2( center.X + w / 2f, center.Y - h / 2f );
			var ttl = Vector2.Transform( tl, transformMatrix );
			var ttr = Vector2.Transform( tr, transformMatrix );
			*/

			var x = new Vector2( Mathf.cos( angle ), Mathf.sin( angle ) );
			var y = new Vector2( -Mathf.sin( angle ), Mathf.cos( angle ) );

			x *= width / 2;
			y *= height / 2;

			_points[0] = -x - y;
			_points[1] = x - y;
			_points[2] = x + y;
			_points[3] = -x + y;

			computeEdges();
		}


		/// <summary>
		/// Updates the edges after the corners move. Assumes the corners actually form a rectangle.
		/// </summary>
		void computeEdges()
		{
			_edges[0] = _points[1] - _points[0];
			_edges[1] = _points[3] - _points[0];

			// make the length of each axis 1/edge length so we know any dot product must be less than 1 to fall within the edge.
			for( var i = 0; i < 2; ++i )
			{
				_edges[i] /= _edges[i].LengthSquared();
				_corner0ProjectionsOnEdges[i] = Vector2.Dot( _points[0], _edges[i] );
			}
		}


		/// <summary>
		/// Returns true if other overlaps any dimension of this
		/// </summary>
		/// <returns><c>true</c>, if way was overlaps1ed, <c>false</c> otherwise.</returns>
		/// <param name="other">Other.</param>
		bool overlapsOneWay( OrientedBoxCollider other )
		{
			for( var i = 0; i < 2; i++ )
			{
				var t = Vector2.Dot( other._points[0], _edges[i] );

				// find the extent of box 2 on axis a
				var tMin = t;
				var tMax = t;

				for( var j = 1; j < 4; j++ )
				{
					t = Vector2.Dot( other._points[j], _edges[i] );
					if( t < tMin )
						tMin = t;
					else if( t > tMax )
						tMax = t;
				}

				// we have to subtract off the origin
				// See if [tMin, tMax] intersects [0, 1]
				if( ( tMin > 1 + _corner0ProjectionsOnEdges[i] ) || ( tMax < _corner0ProjectionsOnEdges[i] ) )
				{
					// there was no intersection along this dimension so the boxes cannot possibly overlap.
					return false;
				}
			}

			// there was no dimension along which there is no intersection. Therefore the boxes overlap.
			return true;
		}


		public void moveTo( Vector2 newCenter )
		{
			var centroid = ( _points[0] + _points[1] + _points[2] + _points[3] ) / 4;
			var translation = newCenter - centroid;

			for( var i = 0; i < 4; i++ )
				_points[i] += translation;

			computeEdges();
		}


		/// <summary>
		/// Returns true if the intersection of the boxes is non-empty
		/// </summary>
		/// <param name="other">Other.</param>
		public bool overlaps( OrientedBoxCollider other )
		{
			return overlapsOneWay( other ) && other.overlapsOneWay( this );
		}


		public override void debugRender( Graphics graphics )
		{
			var rootPosition = new Vector2( entity.position.X + _localPosition.X - _origin.X, entity.position.Y + _localPosition.Y - _origin.Y );

			graphics.drawLine( _points[0] + rootPosition, _points[1] + rootPosition, Color.IndianRed );
			graphics.drawLine( _points[1] + rootPosition, _points[2] + rootPosition, Color.IndianRed );
			graphics.drawLine( _points[2] + rootPosition, _points[3] + rootPosition, Color.IndianRed );
			graphics.drawLine( _points[3] + rootPosition, _points[0] + rootPosition, Color.IndianRed );
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			throw new NotImplementedException();
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			throw new NotImplementedException();
		}


		public override bool collidesWith( CircleCollider circle )
		{
			throw new NotImplementedException();
		}


		public override bool collidesWith( MultiCollider list )
		{
			throw new NotImplementedException();
		}


		public override bool collidesWith( PolygonCollider polygon )
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}

