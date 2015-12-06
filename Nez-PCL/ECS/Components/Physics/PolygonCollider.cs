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

		protected float _rotation;
		public float rotation
		{
			get { return _rotation; }
			set
			{
				if( _rotation != value )
				{
					unregisterColliderWithPhysicsSystem();
					_rotation = value;
					_areBoundsDirty = true;
					_areWorldSpacePointsDirty = true;
					registerColliderWithPhysicsSystem();
				}
			}
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					// we need to find the min/max x/y values
					var minX = float.PositiveInfinity;
					var minY = float.PositiveInfinity;
					var maxX = float.NegativeInfinity;
					var maxY = float.NegativeInfinity;

					for( var i = 0; i < worldSpacePoints.Length; i++ )
					{
						var pt = worldSpacePoints[i];

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
		public Vector2[] edges;

		/// <summary>
		/// Polygon points converted to world space with transformations applied
		/// </summary>
		/// <value>The world space points.</value>
		public Vector2[] worldSpacePoints
		{
			get
			{
				if( _areWorldSpacePointsDirty )
				{
					if( _rotation == 0f )
					{
						for( var i = 0; i < _points.Length; i++ )
							_worldSpacePoints[i] = _points[i] + entity.position + _localPosition - _origin;						
					}
					else
					{
						var matrix = transformMatrix;
						Vector2.Transform( _points, ref matrix, _worldSpacePoints );
					}

					_areWorldSpacePointsDirty = false;
					buildEdges();
				}
				return _worldSpacePoints;
			}
		}

		Matrix _transformMatrix;
		protected Matrix transformMatrix
		{
			get
			{
				var worldPosX = entity.position.X + _localPosition.X;
				var worldPosY = entity.position.Y + _localPosition.Y;
				var tempMat = Matrix.Identity;

				// set the reference point taking origin into account
				_transformMatrix = Matrix.CreateTranslation( -_origin.X, -_origin.Y, 0f ); // origin
				Matrix.CreateRotationZ( _rotation, out tempMat ); // rotation
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
				Matrix.CreateTranslation( _origin.X, _origin.Y, 0f, out tempMat ); // translate back from our origin
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );
				Matrix.CreateTranslation( worldPosX, worldPosY, 0f, out tempMat ); // translate to our world space position
				Matrix.Multiply( ref _transformMatrix, ref tempMat, out _transformMatrix );

				return _transformMatrix;
			}
		}

		protected Vector2[] _points;
		protected Vector2[] _worldSpacePoints;

		/// <summary>
		/// Flag indicating if we need to recalculate the worldSpacePoints. Any change in position or entity.position dirties the flag.
		/// Technically, this should dirty for origin/localPosition changes but if Entity is used to move it will properly get updated.
		/// </summary>
		bool _areWorldSpacePointsDirty = true;


		protected PolygonCollider()
		{}


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
				

			edges = new Vector2[_points.Length];
			_worldSpacePoints = new Vector2[_points.Length];
		}


		public override void onEntityPositionChanged()
		{
			base.onEntityPositionChanged();
			_areWorldSpacePointsDirty = true;
		}


		protected virtual void buildEdges()
		{
			Vector2 p1;
			Vector2 p2;

			for( var i = 0; i < worldSpacePoints.Length; i++ )
			{
				p1 = worldSpacePoints[i];
				if( i + 1 >= worldSpacePoints.Length )
					p2 = worldSpacePoints[0];
				else
					p2 = worldSpacePoints[i + 1];

				edges[i] = p2 - p1;
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
			graphics.drawPolygon( Vector2.Zero, worldSpacePoints, Color.DarkRed, false );
			graphics.drawPixel( entity.position + _localPosition - _origin, Color.Yellow, 2 );
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

