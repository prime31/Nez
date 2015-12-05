using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;


namespace Nez
{
	/// <summary>
	/// Polygons should be defined in clockwise fashion.
	/// </summary>
	public class PolygonCollider : Collider
	{
		public override float width
		{
			get { return bounds.Width; }
			set { throw new NotSupportedException(); }
		}

		public override float height
		{
			get { return bounds.Height; }
			set { throw new NotSupportedException(); }
		}

		// TODO: should this use world space points? It is used in Collisions.polygonToPolygon
		public Vector2 center
		{
			get
			{
				float totalX = 0;
				float totalY = 0;
				for( var i = 0; i < _points.Length; i++ )
				{
					totalX += _points[i].X;
					totalY += _points[i].Y;
				}

				return new Vector2( totalX / (float)_points.Length, totalY / (float)_points.Length );
			}
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					var positionOffset = new Vector2( entity.position.X + _localPosition.X - _origin.X, entity.position.Y + _localPosition.Y - _origin.Y );

					// we need to find the min/max x/y values
					var minX = float.PositiveInfinity;
					var minY = float.PositiveInfinity;
					var maxX = float.NegativeInfinity;
					var maxY = float.NegativeInfinity;

					for( var i = 0; i < _points.Length; i++ )
					{
						var pt = _points[i] + positionOffset;

						if( pt.X < minX )
							minX = pt.X;
						if( pt.X > maxX )
							maxX = pt.X;

						if( pt.Y < minY )
							minY = pt.Y;
						if( pt.Y > maxY )
							maxY = pt.Y;
					}

					_bounds = RectangleExtension.fromMinMaxPoints( new Point( (int)minX, (int)minY ), new Point( (int)maxX, (int)maxY ) );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Polygon edges
		/// </summary>
		/// <value>The edges.</value>
		public Vector2[] edges { get { return _edges; } }

		/// <summary>
		/// Polygon points in object space
		/// </summary>
		/// <value>The local points.</value>
		public Vector2[] localPoints { get { return _points; } }

		/// <summary>
		/// Polygon points converted to world space
		/// </summary>
		/// <value>The world space points.</value>
		public Vector2[] worldSpacePoints
		{
			get
			{
				if( _areWorldSpacePointsDirty )
				{
					for( var i = 0; i < _points.Length; i++ )
						_worldSpacePoints[i] = _points[i] + entity.position + _localPosition - _origin;

					_areWorldSpacePointsDirty = false;
				}
				return _worldSpacePoints;
			}
		}

		Vector2[] _points;
		Vector2[] _edges;
		Vector2[] _worldSpacePoints;

		/// <summary>
		/// Flag indicating if we need to recalculate the worldSpacePoints. Any change in position or entity.position dirties the flag.
		/// Technically, this should dirty for origin/localPosition changes but if Entity is used to move it will properly get updated.
		/// </summary>
		bool _areWorldSpacePointsDirty = true;


		public PolygonCollider( Vector2[] points )
		{
			// first and last point must be the same so that we end up with a closed polygon
			var isPolygonClosed = true;
			if( points[0] != points[points.Length - 1] )
				isPolygonClosed = false;

			// create the array with an extra element if we need to close the poly
			_points = new Vector2[ isPolygonClosed ? points.Length : points.Length + 1];

			// copy our points over
			for( int i = 0; i < points.Length; i++ )
				_points[i] = points[i];

			// close the polygon if necessary
			if( !isPolygonClosed )
				_points[_points.Length - 1] = points[0];
				

			_edges = new Vector2[_points.Length];
			_worldSpacePoints = new Vector2[_points.Length];
			buildEdges();
		}


		public override void onEntityPositionChanged()
		{
			base.onEntityPositionChanged();
			_areWorldSpacePointsDirty = true;
		}


		void buildEdges()
		{
			Vector2 p1;
			Vector2 p2;

			for( var i = 0; i < _points.Length; i++ )
			{
				p1 = _points[i];
				if( i + 1 >= _points.Length )
					p2 = _points[0];
				else
					p2 = _points[i + 1];

				_edges[i] = p2 - p1;
			}
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			throw new NotSupportedException();
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			throw new NotSupportedException();
		}


		public override bool collidesWith( CircleCollider circle )
		{
			return Collisions.polygonToCircle( this, circle.bounds.getCenter(), circle._radius );
		}


		public override bool collidesWith( MultiCollider list )
		{
			throw new NotSupportedException();
		}


		public override bool collidesWith( PolygonCollider polygon )
		{
			var result = Collisions.polygonToPolygon( this, polygon );
			return result.intersects;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.drawPolygon( entity.position + _localPosition, _points, Color.DarkRed, false );
		}


		public override string ToString()
		{
			var builder = new StringBuilder();
			for( var i = 0; i < _points.Length; i++ )
			{
				if( i > 0 )
					builder.Append( " " );
				builder.AppendFormat( "{{{0}}}", _points[i].ToString() );
			}

			return builder.ToString();
		}

	}
}

