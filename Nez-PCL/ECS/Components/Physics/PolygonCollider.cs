using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;


namespace Nez
{
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

		public Vector2 center
		{
			get
			{
				float totalX = 0;
				float totalY = 0;
				for( var i = 0; i < _points.Count; i++ )
				{
					totalX += _points[i].X;
					totalY += _points[i].Y;
				}

				return new Vector2( totalX / (float)_points.Count, totalY / (float)_points.Count );
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

					for( var i = 0; i < _points.Count; i++ )
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
		public List<Vector2> edges { get { return _edges; } }

		/// <summary>
		/// Polygon points in object space
		/// </summary>
		/// <value>The local points.</value>
		public List<Vector2> localPoints { get { return _points; } }

		/// <summary>
		/// Polygon points converted to world space
		/// </summary>
		/// <value>The world space points.</value>
		public List<Vector2> worldSpacePoints
		{
			get
			{
				if( _areWorldSpacePointsDirty )
				{
					_worldSpacePoints.Clear();

					for( var i = 0; i < _points.Count; i++ )
						_worldSpacePoints.Add( _points[i] + entity.position + _localPosition - _origin );

					_areWorldSpacePointsDirty = false;
				}
				return _worldSpacePoints;
			}
		}

		List<Vector2> _points = new List<Vector2>();
		List<Vector2> _edges = new List<Vector2>();
		List<Vector2> _worldSpacePoints = new List<Vector2>();

		/// <summary>
		/// Flag indicating if we need to recalculate the worldSpacePoints. Any change in position or entity.position dirties the flag.
		/// Technically, this should dirty for origin/localPosition changes but if Entity is used to move it will properly get updated.
		/// </summary>
		bool _areWorldSpacePointsDirty = true;


		public PolygonCollider( List<Vector2> points )
		{
			_points = points;
			// first and last point must be the same so that we end up with a closed polygon
			if( _points[0] != _points[points.Count - 1] )
				_points.Add( _points[0] );
			
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
			_edges.Clear();

			for( var i = 0; i < _points.Count; i++ )
			{
				p1 = _points[i];
				if( i + 1 >= _points.Count )
					p2 = _points[0];
				else
					p2 = _points[i + 1];

				_edges.Add( p2 - p1 );
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
			return result.intersect || result.willIntersect;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.drawPolygon( entity.position + _localPosition, _points, Color.DarkRed, false );
		}


		public override string ToString()
		{
			var builder = new StringBuilder();
			for( var i = 0; i < _points.Count; i++ )
			{
				if( i > 0 )
					builder.Append( " " );
				builder.AppendFormat( "{{{0}}}", _points[i].ToString() );
			}

			return builder.ToString();
		}

	}
}

