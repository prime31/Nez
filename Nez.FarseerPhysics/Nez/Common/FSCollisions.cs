//#define DEBUG_FSCOLLISIONS // uncomment to enable Debug of collision points and normals

using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using FSTransform = FarseerPhysics.Common.Transform;


namespace Nez.Farseer
{
	/// <summary>
	/// a collection of helper methods for doing manual collision detection. Why would you want such a thing with a physics engine? The big
	/// reason is so that you can have a fully kinematic Body and retain full and complete control over its movement while it can still
	/// interact with any other dynamic Bodies.
	/// </summary>
	public static class FSCollisions
	{
		static Manifold _manifold;


		/// <summary>
		/// tests for an overlap of shapeA and shapeB. Returns false if both Shapes are chains or if they are not overlapping.
		/// </summary>
		/// <returns><c>true</c>, if overlap was tested, <c>false</c> otherwise.</returns>
		/// <param name="shapeA">Shape a.</param>
		/// <param name="shapeB">Shape b.</param>
		/// <param name="transformA">Transform a.</param>
		/// <param name="transformB">Transform b.</param>
		public static bool TestOverlap(Shape shapeA, Shape shapeB, ref FSTransform transformA,
		                               ref FSTransform transformB)
		{
			if (shapeA.ChildCount == 1 && shapeB.ChildCount == 1)
				return Collision.TestOverlap(shapeA, 0, shapeB, 0, ref transformA, ref transformB);

			if (shapeA.ChildCount > 1)
			{
				for (var i = 0; i < shapeA.ChildCount; i++)
				{
					if (Collision.TestOverlap(shapeA, i, shapeB, 0, ref transformA, ref transformB))
						return true;
				}
			}

			if (shapeB.ChildCount > 1)
			{
				for (var i = 0; i < shapeB.ChildCount; i++)
				{
					if (Collision.TestOverlap(shapeA, 0, shapeB, i, ref transformA, ref transformB))
						return true;
				}
			}

			return false;
		}


		/// <summary>
		/// checks to see if fixtureA with motion applied (delta movement vector) collides with any collider. If it does, true will be
		/// returned and result will be populated with collision data. Motion will be set to the maximum distance the Body can travel
		/// before colliding.
		/// </summary>
		/// <returns><c>true</c>, if fixtures was collided, <c>false</c> otherwise.</returns>
		/// <param name="fixtureA">Fixture a.</param>
		/// <param name="motion">delta movement in simulation space</param>
		/// <param name="fixtureB">Fixture b.</param>
		/// <param name="result">Result.</param>
		public static bool CollideFixtures(Fixture fixtureA, ref Vector2 motion, Fixture fixtureB,
		                                   out FSCollisionResult result)
		{
			// gather our transforms and adjust fixtureA's transform to account for the motion so we check for the collision at its new location
			FSTransform transformA;
			fixtureA.Body.GetTransform(out transformA);
			transformA.P += motion;

			FSTransform transformB;
			fixtureB.Body.GetTransform(out transformB);

			if (CollideFixtures(fixtureA, ref transformA, fixtureB, ref transformB, out result))
			{
				motion += result.MinimumTranslationVector;
				return true;
			}

			return false;
		}


		/// <summary>
		/// checks for collisions between two Fixtures. Note that the first Fixture must have a Circle/PolygonShape and one of the Fixtures must be
		/// static.
		/// </summary>
		/// <returns><c>true</c>, if shapes was collided, <c>false</c> otherwise.</returns>
		/// <param name="fixtureA">Fixture a.</param>
		/// <param name="fixtureB">Fixture b.</param>
		/// <param name="result">Result.</param>
		public static bool CollideFixtures(Fixture fixtureA, Fixture fixtureB, out FSCollisionResult result)
		{
			// gather our transforms
			FSTransform transformA;
			fixtureA.Body.GetTransform(out transformA);

			FSTransform transformB;
			fixtureB.Body.GetTransform(out transformB);

			return CollideFixtures(fixtureA, ref transformA, fixtureB, ref transformB, out result);
		}


		/// <summary>
		/// checks for collisions between two Fixtures. Note that the first Fixture must have a Circle/PolygonShape and one of the Fixtures must be
		/// static.
		/// </summary>
		/// <returns><c>true</c>, if fixtures was collided, <c>false</c> otherwise.</returns>
		/// <param name="fixtureA">Fixture a.</param>
		/// <param name="transformA">Transform a.</param>
		/// <param name="fixtureB">Fixture b.</param>
		/// <param name="transformB">Transform b.</param>
		/// <param name="result">Result.</param>
		public static bool CollideFixtures(Fixture fixtureA, ref FSTransform transformA, Fixture fixtureB,
		                                   ref FSTransform transformB, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			result.Fixture = fixtureB;

			// we need at least one static fixture
			if (!fixtureA.Body.IsStatic && !fixtureB.Body.IsStatic)
			{
				// if the body is dyanmic and asleep wake it up
				if (fixtureB.Body.IsDynamic && !fixtureB.Body.IsAwake)
					fixtureB.Body.IsAwake = true;
				return false;
			}

			// check normal collision filtering
			if (!ContactManager.ShouldCollide(fixtureA, fixtureB))
				return false;

			// check user filtering
			if (fixtureA.Body.World.ContactManager.OnContactFilter != null &&
			    !fixtureA.Body.World.ContactManager.OnContactFilter(fixtureA, fixtureB))
				return false;

			// we only handle Circle or Polygon collisions
			if (fixtureA.Shape is CircleShape)
			{
				if (fixtureB.Shape is CircleShape)
					return CollideCircles(fixtureA.Shape as CircleShape, ref transformA, fixtureB.Shape as CircleShape,
						ref transformB, out result);

				if (fixtureB.Shape is PolygonShape)
					return CollidePolygonCircle(fixtureB.Shape as PolygonShape, ref transformB,
						fixtureA.Shape as CircleShape, ref transformA, out result);

				if (fixtureB.Shape is EdgeShape)
					return CollideEdgeAndCircle(fixtureB.Shape as EdgeShape, ref transformB,
						fixtureA.Shape as CircleShape, ref transformA, out result);

				if (fixtureB.Shape is ChainShape)
				{
					var chain = fixtureB.Shape as ChainShape;
					for (var i = 0; i < chain.ChildCount; i++)
					{
						var edge = chain.GetChildEdge(i);
						if (CollideEdgeAndCircle(edge, ref transformB, fixtureA.Shape as CircleShape, ref transformA,
							out result))
							return true;
					}
				}
			}

			if (fixtureA.Shape is PolygonShape)
			{
				if (fixtureB.Shape is CircleShape)
				{
					var res = CollidePolygonCircle(fixtureA.Shape as PolygonShape, ref transformA,
						fixtureB.Shape as CircleShape, ref transformB, out result);
					result.InvertResult();
					return res;
				}

				if (fixtureB.Shape is PolygonShape)
					return CollidePolygons(fixtureA.Shape as PolygonShape, ref transformA,
						fixtureB.Shape as PolygonShape, ref transformB, out result);

				if (fixtureB.Shape is EdgeShape)
					return CollideEdgeAndPolygon(fixtureB.Shape as EdgeShape, ref transformB,
						fixtureA.Shape as PolygonShape, ref transformA, out result);

				if (fixtureB.Shape is ChainShape)
				{
					var chain = fixtureB.Shape as ChainShape;
					for (var i = 0; i < chain.ChildCount; i++)
					{
						var edge = chain.GetChildEdge(i);
						if (CollideEdgeAndPolygon(edge, ref transformB, fixtureA.Shape as PolygonShape, ref transformA,
							out result))
							return true;
					}
				}
			}

			return false;
		}


		static bool CollidePolygons(PolygonShape polygonA, ref FSTransform transformA, PolygonShape polygonB,
		                            ref FSTransform transformB, out FSCollisionResult result)
		{
			result = new FSCollisionResult();

			Collision.CollidePolygons(ref _manifold, polygonA as PolygonShape, ref transformA, polygonB as PolygonShape,
				ref transformB);
			if (_manifold.PointCount > 0)
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize(ref _manifold, ref transformA, polygonA.Radius, ref transformB,
					polygonB.Radius, out result.Normal, out points);

				// code adapted from PositionSolverManifold.Initialize
				if (_manifold.Type == ManifoldType.FaceA)
				{
					result.Normal = MathUtils.Mul(transformA.Q, _manifold.LocalNormal);
					var planePoint = MathUtils.Mul(ref transformA, _manifold.LocalPoint);

					var clipPoint = MathUtils.Mul(ref transformB, _manifold.Points[0].LocalPoint);
					var separation = Vector2.Dot(clipPoint - planePoint, result.Normal) - polygonA.Radius -
					                 polygonB.Radius;
					result.Point = clipPoint * FSConvert.SimToDisplay;

					// Ensure normal points from A to B
					Vector2.Negate(ref result.Normal, out result.Normal);

					result.MinimumTranslationVector = result.Normal * -separation;
				}
				else
				{
					result.Normal = MathUtils.Mul(transformB.Q, _manifold.LocalNormal);
					var planePoint = MathUtils.Mul(ref transformB, _manifold.LocalPoint);

					var clipPoint = MathUtils.Mul(ref transformA, _manifold.Points[0].LocalPoint);
					var separation = Vector2.Dot(clipPoint - planePoint, result.Normal) - polygonA.Radius -
					                 polygonB.Radius;
					result.Point = clipPoint * FSConvert.SimToDisplay;

					result.MinimumTranslationVector = result.Normal * -separation;
				}

#if DEBUG_FSCOLLISIONS
				Debug.drawPixel( result.point, 2, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );
#endif

				return true;
			}

			return false;
		}


		static bool CollidePolygonCircle(PolygonShape polygon, ref FSTransform polyTransform, CircleShape circle,
		                                 ref FSTransform circleTransform, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			Collision.CollidePolygonAndCircle(ref _manifold, polygon, ref polyTransform, circle, ref circleTransform);
			if (_manifold.PointCount > 0)
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize(ref _manifold, ref polyTransform, polygon.Radius,
					ref circleTransform, circle.Radius, out result.Normal, out points);

				//var circleInPolySpace = polygonFixture.Body.GetLocalPoint( circleFixture.Body.Position );
				var circleInPolySpace = MathUtils.MulT(ref polyTransform, circleTransform.P);
				var value1 = circleInPolySpace - _manifold.LocalPoint;
				var value2 = _manifold.LocalNormal;
				var separation = circle.Radius - (value1.X * value2.X + value1.Y * value2.Y);

				if (separation <= 0)
					return false;

				result.Point = points[0] * FSConvert.SimToDisplay;
				result.MinimumTranslationVector = result.Normal * separation;

#if DEBUG_FSCOLLISIONS
				Debug.drawPixel( result.point, 2, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );
#endif

				return true;
			}

			return false;
		}


		static bool CollideCircles(CircleShape circleA, ref FSTransform firstTransform, CircleShape circleB,
		                           ref FSTransform secondTransform, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			Collision.CollideCircles(ref _manifold, circleA, ref firstTransform, circleB, ref secondTransform);
			if (_manifold.PointCount > 0)
			{
				// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
				// here because it doesnt return some values we need to calculate separation
				var pointA = MathUtils.Mul(ref firstTransform, _manifold.LocalPoint);
				var pointB = MathUtils.Mul(ref secondTransform, _manifold.Points[0].LocalPoint);

				result.Normal = pointA - pointB;
				Vector2Ext.Normalize(ref result.Normal);

				var cA = pointA - circleA.Radius * result.Normal;
				var cB = pointB + circleB.Radius * result.Normal;
				result.Point = 0.5f * (cA + cB);
				result.Point *= FSConvert.SimToDisplay;

				var separation = Vector2.Dot(pointA - pointB, result.Normal) - circleA.Radius - circleB.Radius;
				result.MinimumTranslationVector = result.Normal * Math.Abs(separation);

#if DEBUG_FSCOLLISIONS
				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );
#endif

				return true;
			}

			return false;
		}


		static bool CollideEdgeAndCircle(EdgeShape edge, ref FSTransform edgeTransform, CircleShape circle,
		                                 ref FSTransform circleTransform, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			Collision.CollideEdgeAndCircle(ref _manifold, edge, ref edgeTransform, circle, ref circleTransform);
			if (_manifold.PointCount > 0)
			{
				// code adapted from PositionSolverManifold.Initialize
				if (_manifold.Type == ManifoldType.Circles)
				{
					// this is essentically directly from ContactSolver.WorldManifold.Initialize. To avoid doing the extra math twice we duplicate this code
					// here because it doesnt return some values we need to calculate separation
					var pointA = MathUtils.Mul(ref edgeTransform, _manifold.LocalPoint);
					var pointB = MathUtils.Mul(ref circleTransform, _manifold.Points[0].LocalPoint);

					result.Normal = pointA - pointB;
					Vector2Ext.Normalize(ref result.Normal);

					var cA = pointA - edge.Radius * result.Normal;
					var cB = pointB + circle.Radius * result.Normal;
					result.Point = 0.5f * (cA + cB);
					result.Point *= FSConvert.SimToDisplay;

					var separation = Vector2.Dot(pointA - pointB, result.Normal) - edge.Radius - circle.Radius;

					// Ensure normal points from A to B
					Vector2.Negate(ref result.Normal, out result.Normal);
					result.MinimumTranslationVector = result.Normal * Math.Abs(separation);
				}
				else // FaceA
				{
					result.Normal = MathUtils.Mul(edgeTransform.Q, _manifold.LocalNormal);
					var planePoint = MathUtils.Mul(ref edgeTransform, _manifold.LocalPoint);

					var clipPoint = MathUtils.Mul(ref circleTransform, _manifold.Points[0].LocalPoint);
					var separation = Vector2.Dot(clipPoint - planePoint, result.Normal) - edge.Radius - circle.Radius;
					result.Point = (clipPoint - result.Normal * circle.Radius) * FSConvert.SimToDisplay;

					result.MinimumTranslationVector = result.Normal * -separation;
				}

#if DEBUG_FSCOLLISIONS
				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );
#endif

				return true;
			}

			return false;
		}


		static bool CollideEdgeAndPolygon(EdgeShape edge, ref FSTransform edgeTransform, PolygonShape polygon,
		                                  ref FSTransform polygonTransform, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			Collision.CollideEdgeAndPolygon(ref _manifold, edge, ref edgeTransform, polygon, ref polygonTransform);
			if (_manifold.PointCount > 0)
			{
				FixedArray2<Vector2> points;
				ContactSolver.WorldManifold.Initialize(ref _manifold, ref edgeTransform, edge.Radius,
					ref polygonTransform, polygon.Radius, out result.Normal, out points);

				// code adapted from PositionSolverManifold.Initialize
				if (_manifold.Type == ManifoldType.FaceA)
				{
					result.Normal = MathUtils.Mul(edgeTransform.Q, _manifold.LocalNormal);
					var planePoint = MathUtils.Mul(ref edgeTransform, _manifold.LocalPoint);

					var clipPoint = MathUtils.Mul(ref polygonTransform, _manifold.Points[0].LocalPoint);
					var separation = Vector2.Dot(clipPoint - planePoint, result.Normal) - edge.Radius - polygon.Radius;
					result.Point = clipPoint * FSConvert.SimToDisplay;

					result.MinimumTranslationVector = result.Normal * -separation;
				}
				else
				{
					result.Normal = MathUtils.Mul(polygonTransform.Q, _manifold.LocalNormal);
					var planePoint = MathUtils.Mul(ref polygonTransform, _manifold.LocalPoint);

					var clipPoint = MathUtils.Mul(ref edgeTransform, _manifold.Points[0].LocalPoint);
					var separation = Vector2.Dot(clipPoint - planePoint, result.Normal) - edge.Radius - polygon.Radius;
					result.Point = clipPoint * FSConvert.SimToDisplay;

					// Ensure normal points from A to B
					Vector2.Negate(ref result.Normal, out result.Normal);

					result.MinimumTranslationVector = result.Normal * -separation;
				}

#if DEBUG_FSCOLLISIONS
				Debug.drawPixel( result.point, 5, Color.Red, 0.2f );
				Debug.drawLine( result.point, result.point + result.normal * 20, Color.Yellow, 0.2f );
#endif

				return true;
			}

			return false;
		}
	}
}