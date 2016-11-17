using System;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using FSTransform = FarseerPhysics.Common.Transform;


namespace Nez.Farseer
{
	public static class FarseerCollisions
	{
		static Manifold _manifold;


		/// <summary>
		/// tests for an overlap of shapeA and shapeB. Returns false if both Shapes are chains
		/// </summary>
		/// <returns><c>true</c>, if overlap was tested, <c>false</c> otherwise.</returns>
		/// <param name="shapeA">Shape a.</param>
		/// <param name="shapeB">Shape b.</param>
		/// <param name="transformA">Transform a.</param>
		/// <param name="transformB">Transform b.</param>
		public static bool testOverlap( Shape shapeA, Shape shapeB, ref FSTransform transformA, ref FSTransform transformB )
		{
			if( shapeA.childCount == 1 && shapeB.childCount == 1 )
				return Collision.testOverlap( shapeA, 0, shapeB, 0, ref transformA, ref transformB );

			if( shapeA.childCount > 1 )
			{
				for( var i = 0; i < shapeA.childCount; i++ )
				{
					if( Collision.testOverlap( shapeA, i, shapeB, 0, ref transformA, ref transformB ) )
						return true;
				}
			}

			if( shapeB.childCount > 1 )
			{
				for( var i = 0; i < shapeB.childCount; i++ )
				{
					if( Collision.testOverlap( shapeA, 0, shapeB, i, ref transformA, ref transformB ) )
						return true;
				}
			}

			return false;
		}


		/// <summary>
		/// handles collisions between two Fixtures. Note that the first Fixture must have a Circle/PolygonShape and one of the Fixtures must be
		/// static.
		/// </summary>
		/// <returns><c>true</c>, if shapes was collided, <c>false</c> otherwise.</returns>
		/// <param name="fixtureA">Fixture a.</param>
		/// <param name="fixtureB">Fixture b.</param>
		/// <param name="result">Result.</param>
		public static bool collideShapes( Fixture fixtureA, Fixture fixtureB, out CollisionResult result )
		{
			result = new CollisionResult();

			// we need at least one static fixture
			if( !fixtureA.body.isStatic && !fixtureB.body.isStatic )
			{
				// if the body is dyanmic and asleep wake it up
				if( fixtureB.body.isDynamic && !fixtureB.body.isAwake )
					fixtureB.body.isAwake = true;
				return false;
			}

			// check normal collision filtering
			if( !ContactManager.shouldCollide( fixtureA, fixtureB ) )
				return false;

			// check user filtering
			if( fixtureA.body.world.contactManager.onContactFilter != null && !fixtureA.body.world.contactManager.onContactFilter( fixtureA, fixtureB ) )
				return false;

			// gather our transforms
			FSTransform transformA;
			fixtureA.body.getTransform( out transformA );

			FSTransform transformB;
			fixtureB.body.getTransform( out transformB );

			// we only handle Circle or Polygon collisions
			if( fixtureA.shape is CircleShape )
			{
				if( fixtureB.shape is CircleShape )
					return collideCircles( fixtureA.shape as CircleShape, ref transformA, fixtureB.shape as CircleShape, ref transformB, out result );

				if( fixtureB.shape is PolygonShape )
					return collidePolygonCircle( fixtureB.shape as PolygonShape, ref transformB, fixtureA.shape as CircleShape, ref transformA, out result );

				if( fixtureB.shape is EdgeShape )
					return collideEdgeAndCircle( fixtureB.shape as EdgeShape, ref transformB, fixtureA.shape as CircleShape, ref transformA, out result );

				if( fixtureB.shape is ChainShape )
				{
					var chain = fixtureB.shape as ChainShape;
					for( var i = 0; i < chain.childCount; i++ )
					{
						var edge = chain.getChildEdge( i );
						if( collideEdgeAndCircle( edge, ref transformB, fixtureA.shape as CircleShape, ref transformA, out result ) )
							return true;
					}
				}
			}

			if( fixtureA.shape is PolygonShape )
			{
				if( fixtureB.shape is CircleShape )
				{
					var res = collidePolygonCircle( fixtureA.shape as PolygonShape, ref transformA, fixtureB.shape as CircleShape, ref transformB, out result );
					result.invertResult();
					return res;
				}

				if( fixtureB.shape is PolygonShape )
					return collidePolygons( fixtureA.shape as PolygonShape, ref transformA, fixtureB.shape as PolygonShape, ref transformB, out result );

				if( fixtureB.shape is EdgeShape )
					return collideEdgeAndPolygon( fixtureB.shape as EdgeShape, ref transformB, fixtureA.shape as PolygonShape, ref transformA, out result );

				if( fixtureB.shape is ChainShape )
				{
					var chain = fixtureB.shape as ChainShape;
					for( var i = 0; i < chain.childCount; i++ )
					{
						var edge = chain.getChildEdge( i );
						if( collideEdgeAndPolygon( edge, ref transformB, fixtureA.shape as PolygonShape, ref transformA, out result ) )
							return true;
					}
				}
			}

			return false;
		}


		public static bool collidePolygons( PolygonShape polygonA, ref FSTransform transformA, PolygonShape polygonB, ref FSTransform transformB, out CollisionResult result )
		{
			result = new CollisionResult();

			Collision.collidePolygons( ref _manifold, polygonA as PolygonShape, ref transformA, polygonB as PolygonShape, ref transformB );
			if( _manifold.pointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.initialize( ref _manifold, ref transformA, polygonA.radius, ref transformB, polygonB.radius, out result.normal, out points );

				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.type == ManifoldType.FaceA )
				{
					result.normal = MathUtils.mul( transformA.q, _manifold.localNormal );
					var planePoint = MathUtils.mul( ref transformA, _manifold.localPoint );

					var clipPoint = MathUtils.mul( ref transformB, _manifold.points[0].localPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - polygonA.radius - polygonB.radius;
					result.point = clipPoint * FSConvert.simToDisplay;

					// Ensure normal points from A to B
					Vector2.Negate( ref result.normal, out result.normal );

					result.minimumTranslationVector = result.normal * -separation;
				}
				else
				{
					result.normal = MathUtils.mul( transformB.q, _manifold.localNormal );
					var planePoint = MathUtils.mul( ref transformB, _manifold.localPoint );

					var clipPoint = MathUtils.mul( ref transformA, _manifold.points[0].localPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - polygonA.radius - polygonB.radius;
					result.point = clipPoint * FSConvert.simToDisplay;

					result.minimumTranslationVector = result.normal * -separation;
				}


				Debug.drawPixel( result.point, 2, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );

				return true;
			}

			return false;
		}


		public static bool collidePolygonCircle( PolygonShape polygon, ref FSTransform polyTransform, CircleShape circle, ref FSTransform circleTransform, out CollisionResult result )
		{
			result = new CollisionResult();
			Collision.collidePolygonAndCircle( ref _manifold, polygon, ref polyTransform, circle, ref circleTransform );
			if( _manifold.pointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.initialize( ref _manifold, ref polyTransform, polygon.radius, ref circleTransform, circle.radius, out result.normal, out points );

				//var circleInPolySpace = polygonFixture.Body.GetLocalPoint( circleFixture.Body.Position );
				var circleInPolySpace = MathUtils.mulT( ref polyTransform, circleTransform.p );
				var value1 = circleInPolySpace - _manifold.localPoint;
				var value2 = _manifold.localNormal;
				var separation = circle.radius - ( value1.X * value2.X + value1.Y * value2.Y );

				if( separation <= 0 )
					return false;

				result.point = points[0] * FSConvert.simToDisplay;
				result.minimumTranslationVector = result.normal * separation;

				Debug.drawPixel( result.point, 2, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );

				return true;
			}

			return false;
		}


		public static bool collideCircles( CircleShape circleA, ref FSTransform firstTransform, CircleShape circleB, ref FSTransform secondTransform, out CollisionResult result )
		{
			result = new CollisionResult();
			Collision.collideCircles( ref _manifold, circleA, ref firstTransform, circleB, ref secondTransform );
			if( _manifold.pointCount > 0 )
			{
				// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
				// here because it doesnt return some values we need to calculate separation
				var pointA = MathUtils.mul( ref firstTransform, _manifold.localPoint );
				var pointB = MathUtils.mul( ref secondTransform, _manifold.points[0].localPoint );

				result.normal = pointA - pointB;
				result.normal.Normalize();

				var cA = pointA - circleA.radius * result.normal;
				var cB = pointB + circleB.radius * result.normal;
				result.point = 0.5f * ( cA + cB );
				result.point *= FSConvert.simToDisplay;

				var separation = Vector2.Dot( pointA - pointB, result.normal ) - circleA.radius - circleB.radius;
				result.minimumTranslationVector = result.normal * Math.Abs( separation );

				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );

				return true;
			}

			return false;
		}


		public static bool collideEdgeAndCircle( EdgeShape edge, ref FSTransform edgeTransform, CircleShape circle, ref FSTransform circleTransform, out CollisionResult result )
		{
			result = new CollisionResult();
			Collision.collideEdgeAndCircle( ref _manifold, edge, ref edgeTransform, circle, ref circleTransform );
			if( _manifold.pointCount > 0 )
			{
				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.type == ManifoldType.Circles )
				{
					// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
					// here because it doesnt return some values we need to calculate separation
					var pointA = MathUtils.mul( ref edgeTransform, _manifold.localPoint );
					var pointB = MathUtils.mul( ref circleTransform, _manifold.points[0].localPoint );

					result.normal = pointA - pointB;
					result.normal.Normalize();

					var cA = pointA - edge.radius * result.normal;
					var cB = pointB + circle.radius * result.normal;
					result.point = 0.5f * ( cA + cB );
					result.point *= FSConvert.simToDisplay;

					var separation = Vector2.Dot( pointA - pointB, result.normal ) - edge.radius - circle.radius;

					// Ensure normal points from A to B
					Vector2.Negate( ref result.normal, out result.normal );
					result.minimumTranslationVector = result.normal * Math.Abs( separation );
				}
				else // FaceA
				{
					result.normal = MathUtils.mul( edgeTransform.q, _manifold.localNormal );
					var planePoint = MathUtils.mul( ref edgeTransform, _manifold.localPoint );

					var clipPoint = MathUtils.mul( ref circleTransform, _manifold.points[0].localPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.radius - circle.radius;
					result.point = ( clipPoint - result.normal * circle.radius ) * FSConvert.simToDisplay;

					result.minimumTranslationVector = result.normal * -separation;
				}


				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );

				return true;
			}

			return false;
		}


		public static bool collideEdgeAndPolygon( EdgeShape edge, ref FSTransform edgeTransform, PolygonShape polygon, ref FSTransform polygonTransform, out CollisionResult result )
		{
			result = new CollisionResult();
			Collision.collideEdgeAndPolygon( ref _manifold, edge, ref edgeTransform, polygon, ref polygonTransform );
			if( _manifold.pointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.initialize( ref _manifold, ref edgeTransform, edge.radius, ref polygonTransform, polygon.radius, out result.normal, out points );

				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.type == ManifoldType.FaceA )
				{
					result.normal = MathUtils.mul( edgeTransform.q, _manifold.localNormal );
					var planePoint = MathUtils.mul( ref edgeTransform, _manifold.localPoint );

					var clipPoint = MathUtils.mul( ref polygonTransform, _manifold.points[0].localPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.radius - polygon.radius;
					result.point = clipPoint * FSConvert.simToDisplay;

					result.minimumTranslationVector = result.normal * -separation;
				}
				else
				{
					result.normal = MathUtils.mul( polygonTransform.q, _manifold.localNormal );
					var planePoint = MathUtils.mul( ref polygonTransform, _manifold.localPoint );

					var clipPoint = MathUtils.mul( ref edgeTransform, _manifold.points[0].localPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.radius - polygon.radius;
					result.point = clipPoint * FSConvert.simToDisplay;

					// Ensure normal points from A to B
					Vector2.Negate( ref result.normal, out result.normal );

					result.minimumTranslationVector = result.normal * -separation;
				}

				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );

				return true;
			}

			return false;
		}

	}
}
