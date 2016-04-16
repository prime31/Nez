using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace Nez.PhysicsShapes
{
	public class Circle : Shape
	{
		public float radius;


		public Circle( float radius )
		{
			this.radius = radius;
		}


		internal override void recalculateBounds( Collider collider )
		{
			position = collider.absolutePosition;
			bounds = new RectangleF( collider.entity.transform.position.X + collider.localOffset.X + collider.origin.X - radius, collider.entity.transform.position.Y + collider.localOffset.Y + collider.origin.Y - radius, radius * 2f, radius * 2f );
		}


		internal void recalculateBounds( float radius, Vector2 position )
		{
			this.radius = radius;
			this.position = position;
			bounds = new RectangleF( position.X - radius, position.Y - radius, radius * 2f, radius * 2f );
		}


		public override bool overlaps( Shape other )
		{
			CollisionResult result;
			if( other is Box )
				return Collisions.rectToCircle( ref other.bounds, position, radius );

			if( other is Circle )
				return Collisions.circleToCircle( position, radius, other.position, ( other as Circle ).radius );

			if( other is Polygon )
				return ShapeCollisions.circleToPolygon( this, other as Polygon, out result );

			throw new NotImplementedException( string.Format( "overlaps of Circle to {0} are not supported", other ) );
		}


		public override bool collidesWithShape( Shape other, out CollisionResult result )
		{
			if( other is Box )
				return ShapeCollisions.circleToBox( this, other as Box, out result );

			if( other is Circle )
				return ShapeCollisions.circleToCircle( this, other as Circle, out result );

			if( other is Polygon )
				return ShapeCollisions.circleToPolygon( this, other as Polygon, out result );

			throw new NotImplementedException( string.Format( "Collisions of Circle to {0} are not supported", other ) );
		}


		public override bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit )
		{
			hit = new RaycastHit();
			return ShapeCollisions.lineToCircle( start, end, this, out hit );
		}


		/// <summary>
		/// Gets the point at the edge of this <see cref="Circle"/> from the provided angle
		/// </summary>
		/// <param name="angle">an angle in radians</param>
		/// <returns><see cref="Vector2"/> representing the point on this <see cref="Circle"/>'s surface at the specified angle</returns>
		public Vector2 getPointAlongEdge( float angle )
		{
			return new Vector2( position.X + ( radius * Mathf.cos( angle ) ), position.Y + ( radius * Mathf.sin( angle ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool contains( float x, float y )
		{
			return contains( new Vector2( x, y ) );
		}


		/// <summary>
		/// Gets whether or not the provided point lie within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="oint">the point</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool contains( Vector2 point )
		{
			return ( ( point - position ).LengthSquared() <= radius * radius );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="point">Point.</param>
		public bool contains( ref Vector2 point )
		{
			return ( point - position ).LengthSquared() <= radius * radius;
		}

	}
}

