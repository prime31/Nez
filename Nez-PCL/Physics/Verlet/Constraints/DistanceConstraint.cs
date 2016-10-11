using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Verlet
{
	/// <summary>
	/// maintains a specified distance betweeen two Particles. The stiffness adjusts how rigid or springy the constraint will be.
	/// </summary>
	public class DistanceConstraint : Constraint
	{
		/// <summary>
		/// [0-1]. the stiffness of the Constraint. Lower values are more springy and higher are more rigid.
		/// </summary>
		public float stiffness;

		/// <summary>
		/// the resting distnace of the Constraint. It will always try to get to this distance.
		/// </summary>
		public float restingDistance;

		/// <summary>
		/// if the ratio of the current distance / restingDistance is greater than tearSensitivity the Constaint will be removed. Values
		/// should be above 1 and higher values mean rupture wont occur until the Constaint is stretched further.
		/// </summary>
		public float tearSensitivity = float.PositiveInfinity;

		/// <summary>
		/// sets whether collisions should be approximated by points. This should be used for Constraints that need to collided on both
		/// sides. SAT only works with single sided collisions.
		/// </summary>
		public bool shouldApproximateCollisionsWithPoints;

		/// <summary>
		/// if shouldApproximateCollisionsWithPoints is true, this controls how accurate the collisions check will be. Higher numbers mean
		/// more collisions checks.
		/// </summary>
		public int totalPointsToApproximateCollisionsWith = 5;

		/// <summary>
		/// the first Particle in the Constraint
		/// </summary>
		Particle _particleOne;

		/// <summary>
		/// the second particle in the Constraint
		/// </summary>
		Particle _particleTwo;

		/// <summary>
		/// Polygon shared amongst all DistanceConstraints. Used for collision detection.
		/// </summary>
		static Polygon _polygon = new Polygon( 2, 1 );


		public DistanceConstraint( Particle first, Particle second, float stiffness, float distance = -1 )
		{
			_particleOne = first;
			_particleTwo = second;
			this.stiffness = stiffness;

			if( distance > -1 )
				restingDistance = distance;
			else
				restingDistance = Vector2.Distance( first.position, second.position );
		}


		/// <summary>
		/// creates a faux angle constraint by figuring out the required distance from a to c for the given angle
		/// </summary>
		/// <param name="a">The alpha component.</param>
		/// <param name="center">Center.</param>
		/// <param name="c">C.</param>
		/// <param name="stiffness">Stiffness.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		public static DistanceConstraint create( Particle a, Particle center, Particle c, float stiffness, float angleInDegrees )
		{
			var aToCenter = Vector2.Distance( a.position, center.position );
			var cToCenter = Vector2.Distance( c.position, center.position );
			var distance = Mathf.sqrt( aToCenter * aToCenter + cToCenter * cToCenter - ( 2 * aToCenter * cToCenter * Mathf.cos( angleInDegrees * Mathf.deg2Rad ) ) );

			return new DistanceConstraint( a, c, stiffness, distance );
		}


		/// <summary>
		/// sets the tear sensitivity. if the ratio of the current distance / restingDistance is greater than tearSensitivity the
		/// Constaint will be removed
		/// </summary>
		/// <returns>The tear sensitvity.</returns>
		/// <param name="tearSensitivity">Tear sensitivity.</param>
		public DistanceConstraint setTearSensitivity( float tearSensitivity )
		{
			this.tearSensitivity = tearSensitivity;
			return this;
		}


		/// <summary>
		/// sets whether this Constraint should collide with standard Colliders
		/// </summary>
		/// <returns>The collides with colliders.</returns>
		/// <param name="collidesWithColliders">If set to <c>true</c> collides with colliders.</param>
		public DistanceConstraint setCollidesWithColliders( bool collidesWithColliders )
		{
			this.collidesWithColliders = collidesWithColliders;
			return this;
		}


		/// <summary>
		/// sets whether collisions should be approximated by points. This should be used for Constraints that need to collided on both
		/// sides. SAT only works with single sided collisions.
		/// </summary>
		/// <returns>The should approximate collisions with points.</returns>
		/// <param name="shouldApproximateCollisionsWithPoints">If set to <c>true</c> should approximate collisions with points.</param>
		public DistanceConstraint setShouldApproximateCollisionsWithPoints( bool shouldApproximateCollisionsWithPoints )
		{
			this.shouldApproximateCollisionsWithPoints = shouldApproximateCollisionsWithPoints;
			return this;
		}


		public override void solve()
		{
			// calculate the distance between the two Particles
			var diff = _particleOne.position - _particleTwo.position;
			var d = diff.Length();

			// find the difference, or the ratio of how far along the restingDistance the actual distance is.
			var difference = ( restingDistance - d ) / d;

			// if the distance is more than tearSensitivity we remove the Constraint
			if( d / restingDistance > tearSensitivity )
			{
				composite.removeConstraint( this );
				return;
			}

			// inverse the mass quantities
			var im1 = 1f / _particleOne.mass;
			var im2 = 1f / _particleTwo.mass;
			var scalarP1 = ( im1 / ( im1 + im2 ) ) * stiffness;
			var scalarP2 = stiffness - scalarP1;

			// push/pull based on mass
			// heavier objects will be pushed/pulled less than attached light objects
			_particleOne.position += diff * scalarP1 * difference;
			_particleTwo.position -= diff * scalarP2 * difference;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override void handleCollisions( int collidesWithLayers )
		{
			if( shouldApproximateCollisionsWithPoints )
			{
				approximateCollisionsWithPoints( collidesWithLayers );
				return;
			}

			// get a proper bounds for our line and update the Polygons bounds
			var minX = Math.Min( _particleOne.position.X, _particleTwo.position.X );
			var maxX = Math.Max( _particleOne.position.X, _particleTwo.position.X );
			var minY = Math.Min( _particleOne.position.Y, _particleTwo.position.Y );
			var maxY = Math.Max( _particleOne.position.Y, _particleTwo.position.Y );
			_polygon.bounds = RectangleF.fromMinMax( minX, minY, maxX, maxY );

			Vector2 midPoint;
			preparePolygonForCollisionChecks( out midPoint );

			var colliders = Physics.boxcastBroadphase( ref _polygon.bounds, collidesWithLayers );
			foreach( var collider in colliders )
			{
				CollisionResult result;
				if( _polygon.collidesWithShape( collider.shape, out result ) )
				{
					// TODO: do we need this?
					// special care needs to be taken for a Circle. Since our Polygon is only a single segment with a normal off both edges
					// we could end up with a backwards result. We fix that by seeing if the normal is in the direction of the Circle
					// center. If it is, we flip the result.
					//if( collider.shape is Circle )
					//{
					//	var dot = Vector2.Dot( midPoint - collider.shape.position, result.normal );
					//	if( dot < 0 )
					//		result.invertResult();
					//}

					_particleOne.position -= result.minimumTranslationVector;
					_particleTwo.position -= result.minimumTranslationVector;
				}
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void approximateCollisionsWithPoints( int collidesWithLayers )
		{
			Vector2 pt;
			for( var j = 0; j < totalPointsToApproximateCollisionsWith - 1; j++ )
			{
				pt = Vector2.Lerp( _particleOne.position, _particleTwo.position, ( j + 1 ) / (float)totalPointsToApproximateCollisionsWith );
				var collidedCount = Physics.overlapCircleAll( pt, 3, World._colliders, collidesWithLayers );
				for( var i = 0; i < collidedCount; i++ )
				{
					var collider = World._colliders[i];
					CollisionResult collisionResult;
					if( collider.shape.pointCollidesWithShape( pt, out collisionResult ) )
					{
						_particleOne.position -= collisionResult.minimumTranslationVector;
						_particleTwo.position -= collisionResult.minimumTranslationVector;
					}
				}
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void preparePolygonForCollisionChecks( out Vector2 midPoint )
		{
			// set our Polygon points
			midPoint = Vector2.Lerp( _particleOne.position, _particleTwo.position, 0.5f );
			_polygon.position = midPoint;
			_polygon.points[0] = _particleOne.position - _polygon.position;
			_polygon.points[1] = _particleTwo.position - _polygon.position;
			_polygon.recalculateCenterAndEdgeNormals();
		}


		public override void debugRender( Batcher batcher )
		{
			batcher.drawLine( _particleOne.position, _particleTwo.position, DefaultColors.verletConstraintEdge );
		}

	}
}
