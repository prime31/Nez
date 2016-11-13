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
			if( shapeA.ChildCount == 1 && shapeB.ChildCount == 1 )
				return Collision.TestOverlap( shapeA, 0, shapeB, 0, ref transformA, ref transformB );

			if( shapeA.ChildCount > 1 )
			{
				for( var i = 0; i < shapeA.ChildCount; i++ )
				{
					if( Collision.TestOverlap( shapeA, i, shapeB, 0, ref transformA, ref transformB ) )
						return true;
				}
			}

			if( shapeB.ChildCount > 1 )
			{
				for( var i = 0; i < shapeB.ChildCount; i++ )
				{
					if( Collision.TestOverlap( shapeA, 0, shapeB, i, ref transformA, ref transformB ) )
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
			if( !fixtureA.Body.IsStatic && !fixtureB.Body.IsStatic )
			{
				// if the body is dyanmic and asleep wake it up
				if( fixtureB.Body.IsDynamic && !fixtureB.Body.Awake )
					fixtureB.Body.Awake = true;
				return false;
			}

			// check normal collision filtering
			if( !ContactManager.ShouldCollide( fixtureA, fixtureB ) )
				return false;

			// check user filtering
			if( fixtureA.Body.World.ContactManager.ContactFilter != null && !fixtureA.Body.World.ContactManager.ContactFilter( fixtureA, fixtureB ) )
				return false;

			// gather our transforms
			FSTransform transformA;
			fixtureA.Body.GetTransform( out transformA );

			FSTransform transformB;
			fixtureB.Body.GetTransform( out transformB );

			// we only handle Circle or Polygon collisions
			if( fixtureA.Shape is CircleShape )
			{
				if( fixtureB.Shape is CircleShape )
					return collideCircles( fixtureA.Shape as CircleShape, ref transformA, fixtureB.Shape as CircleShape, ref transformB, out result );

				if( fixtureB.Shape is PolygonShape )
					return collidePolygonCircle( fixtureB.Shape as PolygonShape, ref transformB, fixtureA.Shape as CircleShape, ref transformA, out result );

				if( fixtureB.Shape is EdgeShape )
					return collideEdgeAndCircle( fixtureB.Shape as EdgeShape, ref transformB, fixtureA.Shape as CircleShape, ref transformA, out result );

				if( fixtureB.Shape is ChainShape )
				{
					var chain = fixtureB.Shape as ChainShape;
					for( var i = 0; i < chain.ChildCount; i++ )
					{
						var edge = chain.GetChildEdge( i );
						if( collideEdgeAndCircle( edge, ref transformB, fixtureA.Shape as CircleShape, ref transformA, out result ) )
							return true;
					}
				}
			}

			if( fixtureA.Shape is PolygonShape )
			{
				if( fixtureB.Shape is CircleShape )
				{
					var res = collidePolygonCircle( fixtureA.Shape as PolygonShape, ref transformA, fixtureB.Shape as CircleShape, ref transformB, out result );
					result.invertResult();
					return res;
				}

				if( fixtureB.Shape is PolygonShape )
					return collidePolygons( fixtureA.Shape as PolygonShape, ref transformA, fixtureB.Shape as PolygonShape, ref transformB, out result );

				if( fixtureB.Shape is EdgeShape )
					return collideEdgeAndPolygon( fixtureB.Shape as EdgeShape, ref transformB, fixtureA.Shape as PolygonShape, ref transformA, out result );

				if( fixtureB.Shape is ChainShape )
				{
					var chain = fixtureB.Shape as ChainShape;
					for( var i = 0; i < chain.ChildCount; i++ )
					{
						var edge = chain.GetChildEdge( i );
						if( collideEdgeAndPolygon( edge, ref transformB, fixtureA.Shape as PolygonShape, ref transformA, out result ) )
							return true;
					}
				}
			}

			return false;
		}


		public static bool collidePolygons( PolygonShape polygonA, ref FSTransform transformA, PolygonShape polygonB, ref FSTransform transformB, out CollisionResult result )
		{
			result = new CollisionResult();

			Collision.CollidePolygons( ref _manifold, polygonA as PolygonShape, ref transformA, polygonB as PolygonShape, ref transformB );
			if( _manifold.PointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize( ref _manifold, ref transformA, polygonA.Radius, ref transformB, polygonB.Radius, out result.normal, out points );

				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.Type == ManifoldType.FaceA )
				{
					result.normal = MathUtils.Mul( transformA.q, _manifold.LocalNormal );
					var planePoint = MathUtils.Mul( ref transformA, _manifold.LocalPoint );

					var clipPoint = MathUtils.Mul( ref transformB, _manifold.Points[0].LocalPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - polygonA.Radius - polygonB.Radius;
					result.point = clipPoint * ConvertUnits.simToDisplay;

					// Ensure normal points from A to B
					Vector2.Negate( ref result.normal, out result.normal );

					result.minimumTranslationVector = result.normal * -separation;
				}
				else
				{
					result.normal = MathUtils.Mul( transformB.q, _manifold.LocalNormal );
					var planePoint = MathUtils.Mul( ref transformB, _manifold.LocalPoint );

					var clipPoint = MathUtils.Mul( ref transformA, _manifold.Points[0].LocalPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - polygonA.Radius - polygonB.Radius;
					result.point = clipPoint * ConvertUnits.simToDisplay;

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
			Collision.CollidePolygonAndCircle( ref _manifold, polygon, ref polyTransform, circle, ref circleTransform );
			if( _manifold.PointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize( ref _manifold, ref polyTransform, polygon.Radius, ref circleTransform, circle.Radius, out result.normal, out points );

				//var circleInPolySpace = polygonFixture.Body.GetLocalPoint( circleFixture.Body.Position );
				var circleInPolySpace = MathUtils.MulT( ref polyTransform, circleTransform.p );
				var value1 = circleInPolySpace - _manifold.LocalPoint;
				var value2 = _manifold.LocalNormal;
				var separation = circle.Radius - ( value1.X * value2.X + value1.Y * value2.Y );

				if( separation <= 0 )
					return false;

				result.point = points[0] * ConvertUnits.simToDisplay;
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
			Collision.CollideCircles( ref _manifold, circleA, ref firstTransform, circleB, ref secondTransform );
			if( _manifold.PointCount > 0 )
			{
				// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
				// here because it doesnt return some values we need to calculate separation
				var pointA = MathUtils.Mul( ref firstTransform, _manifold.LocalPoint );
				var pointB = MathUtils.Mul( ref secondTransform, _manifold.Points[0].LocalPoint );

				result.normal = pointA - pointB;
				result.normal.Normalize();

				var cA = pointA - circleA.Radius * result.normal;
				var cB = pointB + circleB.Radius * result.normal;
				result.point = 0.5f * ( cA + cB );
				result.point *= ConvertUnits.simToDisplay;

				var separation = Vector2.Dot( pointA - pointB, result.normal ) - circleA.Radius - circleB.Radius;
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
			Collision.CollideEdgeAndCircle( ref _manifold, edge, ref edgeTransform, circle, ref circleTransform );
			if( _manifold.PointCount > 0 )
			{
				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.Type == ManifoldType.Circles )
				{
					// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
					// here because it doesnt return some values we need to calculate separation
					var pointA = MathUtils.Mul( ref edgeTransform, _manifold.LocalPoint );
					var pointB = MathUtils.Mul( ref circleTransform, _manifold.Points[0].LocalPoint );

					result.normal = pointA - pointB;
					result.normal.Normalize();

					var cA = pointA - edge.Radius * result.normal;
					var cB = pointB + circle.Radius * result.normal;
					result.point = 0.5f * ( cA + cB );
					result.point *= ConvertUnits.simToDisplay;

					var separation = Vector2.Dot( pointA - pointB, result.normal ) - edge.Radius - circle.Radius;

					// Ensure normal points from A to B
					Vector2.Negate( ref result.normal, out result.normal );
					result.minimumTranslationVector = result.normal * Math.Abs( separation );
				}
				else // FaceA
				{
					result.normal = MathUtils.Mul( edgeTransform.q, _manifold.LocalNormal );
					var planePoint = MathUtils.Mul( ref edgeTransform, _manifold.LocalPoint );

					var clipPoint = MathUtils.Mul( ref circleTransform, _manifold.Points[0].LocalPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.Radius - circle.Radius;
					result.point = ( clipPoint - result.normal * circle.Radius ) * ConvertUnits.simToDisplay;

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
			Collision.CollideEdgeAndPolygon( ref _manifold, edge, ref edgeTransform, polygon, ref polygonTransform );
			if( _manifold.PointCount > 0 )
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize( ref _manifold, ref edgeTransform, edge.Radius, ref polygonTransform, polygon.Radius, out result.normal, out points );

				// code adapted from PositionSolverManifold.Initialize
				if( _manifold.Type == ManifoldType.FaceA )
				{
					result.normal = MathUtils.Mul( edgeTransform.q, _manifold.LocalNormal );
					var planePoint = MathUtils.Mul( ref edgeTransform, _manifold.LocalPoint );

					var clipPoint = MathUtils.Mul( ref polygonTransform, _manifold.Points[0].LocalPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.Radius - polygon.Radius;
					result.point = clipPoint * ConvertUnits.simToDisplay;

					result.minimumTranslationVector = result.normal * -separation;
				}
				else
				{
					result.normal = MathUtils.Mul( polygonTransform.q, _manifold.LocalNormal );
					var planePoint = MathUtils.Mul( ref polygonTransform, _manifold.LocalPoint );

					var clipPoint = MathUtils.Mul( ref edgeTransform, _manifold.Points[0].LocalPoint );
					var separation = Vector2.Dot( clipPoint - planePoint, result.normal ) - edge.Radius - polygon.Radius;
					result.point = clipPoint * ConvertUnits.simToDisplay;

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
