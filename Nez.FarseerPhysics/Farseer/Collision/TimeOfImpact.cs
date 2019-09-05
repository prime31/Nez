/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision
{
	/// <summary>
	/// Input parameters for CalculateTimeOfImpact
	/// </summary>
	public class TOIInput
	{
		public DistanceProxy ProxyA = new DistanceProxy();
		public DistanceProxy ProxyB = new DistanceProxy();
		public Sweep SweepA;
		public Sweep SweepB;
		public float TMax; // defines sweep interval [0, tMax]
	}

	public enum TOIOutputState
	{
		Unknown,
		Failed,
		Overlapped,
		Touching,
		Seperated,
	}

	public struct TOIOutput
	{
		public TOIOutputState State;
		public float T;
	}

	public enum SeparationFunctionType
	{
		Points,
		FaceA,
		FaceB
	}


	public static class SeparationFunction
	{
		[ThreadStatic] static Vector2 _axis;
		[ThreadStatic] static Vector2 _localPoint;
		[ThreadStatic] static DistanceProxy _proxyA;
		[ThreadStatic] static DistanceProxy _proxyB;
		[ThreadStatic] static Sweep _sweepA, _sweepB;
		[ThreadStatic] static SeparationFunctionType _type;


		public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB,
		                       ref Sweep sweepB, float t1)
		{
			_localPoint = Vector2.Zero;
			_proxyA = proxyA;
			_proxyB = proxyB;
			var count = cache.Count;
			Debug.Assert(0 < count && count < 3);

			_sweepA = sweepA;
			_sweepB = sweepB;

			Transform xfA, xfB;
			_sweepA.GetTransform(out xfA, t1);
			_sweepB.GetTransform(out xfB, t1);

			if (count == 1)
			{
				_type = SeparationFunctionType.Points;
				var localPointA = _proxyA.vertices[cache.IndexA[0]];
				var localPointB = _proxyB.vertices[cache.IndexB[0]];
				var pointA = MathUtils.Mul(ref xfA, localPointA);
				var pointB = MathUtils.Mul(ref xfB, localPointB);
				_axis = pointB - pointA;
				Nez.Vector2Ext.Normalize(ref _axis);
			}
			else if (cache.IndexA[0] == cache.IndexA[1])
			{
				// Two points on B and one on A.
				_type = SeparationFunctionType.FaceB;
				var localPointB1 = proxyB.vertices[cache.IndexB[0]];
				var localPointB2 = proxyB.vertices[cache.IndexB[1]];

				var a = localPointB2 - localPointB1;
				_axis = new Vector2(a.Y, -a.X);
				Nez.Vector2Ext.Normalize(ref _axis);
				var normal = MathUtils.Mul(ref xfB.Q, _axis);

				_localPoint = 0.5f * (localPointB1 + localPointB2);
				var pointB = MathUtils.Mul(ref xfB, _localPoint);

				var localPointA = proxyA.vertices[cache.IndexA[0]];
				var pointA = MathUtils.Mul(ref xfA, localPointA);

				var s = Vector2.Dot(pointA - pointB, normal);
				if (s < 0.0f)
					_axis = -_axis;
			}
			else
			{
				// Two points on A and one or two points on B.
				_type = SeparationFunctionType.FaceA;
				var localPointA1 = _proxyA.vertices[cache.IndexA[0]];
				var localPointA2 = _proxyA.vertices[cache.IndexA[1]];

				var a = localPointA2 - localPointA1;
				_axis = new Vector2(a.Y, -a.X);
				Nez.Vector2Ext.Normalize(ref _axis);
				var normal = MathUtils.Mul(ref xfA.Q, _axis);

				_localPoint = 0.5f * (localPointA1 + localPointA2);
				var pointA = MathUtils.Mul(ref xfA, _localPoint);

				var localPointB = _proxyB.vertices[cache.IndexB[0]];
				var pointB = MathUtils.Mul(ref xfB, localPointB);

				var s = Vector2.Dot(pointB - pointA, normal);
				if (s < 0.0f)
					_axis = -_axis;
			}

			//FPE note: the returned value that used to be here has been removed, as it was not used.
		}

		public static float FindMinSeparation(out int indexA, out int indexB, float t)
		{
			Transform xfA, xfB;
			_sweepA.GetTransform(out xfA, t);
			_sweepB.GetTransform(out xfB, t);

			switch (_type)
			{
				case SeparationFunctionType.Points:
				{
					var axisA = MathUtils.MulT(ref xfA.Q, _axis);
					var axisB = MathUtils.MulT(ref xfB.Q, -_axis);

					indexA = _proxyA.GetSupport(axisA);
					indexB = _proxyB.GetSupport(axisB);

					var localPointA = _proxyA.vertices[indexA];
					var localPointB = _proxyB.vertices[indexB];

					var pointA = MathUtils.Mul(ref xfA, localPointA);
					var pointB = MathUtils.Mul(ref xfB, localPointB);

					var separation = Vector2.Dot(pointB - pointA, _axis);
					return separation;
				}

				case SeparationFunctionType.FaceA:
				{
					var normal = MathUtils.Mul(ref xfA.Q, _axis);
					var pointA = MathUtils.Mul(ref xfA, _localPoint);

					var axisB = MathUtils.MulT(ref xfB.Q, -normal);

					indexA = -1;
					indexB = _proxyB.GetSupport(axisB);

					var localPointB = _proxyB.vertices[indexB];
					var pointB = MathUtils.Mul(ref xfB, localPointB);

					var separation = Vector2.Dot(pointB - pointA, normal);
					return separation;
				}

				case SeparationFunctionType.FaceB:
				{
					var normal = MathUtils.Mul(ref xfB.Q, _axis);
					var pointB = MathUtils.Mul(ref xfB, _localPoint);

					var axisA = MathUtils.MulT(ref xfA.Q, -normal);

					indexB = -1;
					indexA = _proxyA.GetSupport(axisA);

					var localPointA = _proxyA.vertices[indexA];
					var pointA = MathUtils.Mul(ref xfA, localPointA);

					var separation = Vector2.Dot(pointA - pointB, normal);
					return separation;
				}

				default:
					Debug.Assert(false);
					indexA = -1;
					indexB = -1;
					return 0.0f;
			}
		}

		public static float Evaluate(int indexA, int indexB, float t)
		{
			Transform xfA, xfB;
			_sweepA.GetTransform(out xfA, t);
			_sweepB.GetTransform(out xfB, t);

			switch (_type)
			{
				case SeparationFunctionType.Points:
				{
					var localPointA = _proxyA.vertices[indexA];
					var localPointB = _proxyB.vertices[indexB];

					var pointA = MathUtils.Mul(ref xfA, localPointA);
					var pointB = MathUtils.Mul(ref xfB, localPointB);
					var separation = Vector2.Dot(pointB - pointA, _axis);

					return separation;
				}
				case SeparationFunctionType.FaceA:
				{
					var normal = MathUtils.Mul(ref xfA.Q, _axis);
					var pointA = MathUtils.Mul(ref xfA, _localPoint);

					var localPointB = _proxyB.vertices[indexB];
					var pointB = MathUtils.Mul(ref xfB, localPointB);

					var separation = Vector2.Dot(pointB - pointA, normal);
					return separation;
				}
				case SeparationFunctionType.FaceB:
				{
					var normal = MathUtils.Mul(ref xfB.Q, _axis);
					var pointB = MathUtils.Mul(ref xfB, _localPoint);

					var localPointA = _proxyA.vertices[indexA];
					var pointA = MathUtils.Mul(ref xfA, localPointA);

					var separation = Vector2.Dot(pointA - pointB, normal);
					return separation;
				}
				default:
					Debug.Assert(false);
					return 0.0f;
			}
		}
	}


	public static class TimeOfImpact
	{
		// CCD via the local separating axis method. This seeks progression
		// by computing the largest time at which separation is maintained.

		[ThreadStatic] public static int TOICalls, TOIIters, TOIMaxIters;
		[ThreadStatic] public static int TOIRootIters, TOIMaxRootIters;
		[ThreadStatic] static DistanceInput _distanceInput;

		/// <summary>
		/// Compute the upper bound on time before two shapes penetrate. Time is represented as
		/// a fraction between [0,tMax]. This uses a swept separating axis and may miss some intermediate,
		/// non-tunneling collision. If you change the time interval, you should call this function
		/// again.
		/// Note: use Distance() to compute the contact point and normal at the time of impact.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="input">The input.</param>
		public static void CalculateTimeOfImpact(out TOIOutput output, TOIInput input)
		{
			if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
				++TOICalls;

			output = new TOIOutput();
			output.State = TOIOutputState.Unknown;
			output.T = input.TMax;

			var sweepA = input.SweepA;
			var sweepB = input.SweepB;

			// Large rotations can make the root finder fail, so we normalize the sweep angles.
			sweepA.Normalize();
			sweepB.Normalize();

			var tMax = input.TMax;

			var totalRadius = input.ProxyA.radius + input.ProxyB.radius;
			var target = Math.Max(Settings.LinearSlop, totalRadius - 3.0f * Settings.LinearSlop);
			const float tolerance = 0.25f * Settings.LinearSlop;
			Debug.Assert(target > tolerance);

			var t1 = 0.0f;
			const int k_maxIterations = 20;
			var iter = 0;

			// Prepare input for distance query.
			_distanceInput = _distanceInput ?? new DistanceInput();
			_distanceInput.ProxyA = input.ProxyA;
			_distanceInput.ProxyB = input.ProxyB;
			_distanceInput.UseRadii = false;

			// The outer loop progressively attempts to compute new separating axes.
			// This loop terminates when an axis is repeated (no progress is made).
			for (;;)
			{
				Transform xfA, xfB;
				sweepA.GetTransform(out xfA, t1);
				sweepB.GetTransform(out xfB, t1);

				// Get the distance between shapes. We can also use the results
				// to get a separating axis.
				_distanceInput.TransformA = xfA;
				_distanceInput.TransformB = xfB;
				DistanceOutput distanceOutput;
				SimplexCache cache;
				Distance.ComputeDistance(out distanceOutput, out cache, _distanceInput);

				// If the shapes are overlapped, we give up on continuous collision.
				if (distanceOutput.Distance <= 0.0f)
				{
					// Failure!
					output.State = TOIOutputState.Overlapped;
					output.T = 0.0f;
					break;
				}

				if (distanceOutput.Distance < target + tolerance)
				{
					// Victory!
					output.State = TOIOutputState.Touching;
					output.T = t1;
					break;
				}

				SeparationFunction.Set(ref cache, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, t1);

				// Compute the TOI on the separating axis. We do this by successively
				// resolving the deepest point. This loop is bounded by the number of vertices.
				var done = false;
				var t2 = tMax;
				var pushBackIter = 0;
				for (;;)
				{
					// Find the deepest point at t2. Store the witness point indices.
					int indexA, indexB;
					var s2 = SeparationFunction.FindMinSeparation(out indexA, out indexB, t2);

					// Is the final configuration separated?
					if (s2 > target + tolerance)
					{
						// Victory!
						output.State = TOIOutputState.Seperated;
						output.T = tMax;
						done = true;
						break;
					}

					// Has the separation reached tolerance?
					if (s2 > target - tolerance)
					{
						// Advance the sweeps
						t1 = t2;
						break;
					}

					// Compute the initial separation of the witness points.
					float s1 = SeparationFunction.Evaluate(indexA, indexB, t1);

					// Check for initial overlap. This might happen if the root finder
					// runs out of iterations.
					if (s1 < target - tolerance)
					{
						output.State = TOIOutputState.Failed;
						output.T = t1;
						done = true;
						break;
					}

					// Check for touching
					if (s1 <= target + tolerance)
					{
						// Victory! t1 should hold the TOI (could be 0.0).
						output.State = TOIOutputState.Touching;
						output.T = t1;
						done = true;
						break;
					}

					// Compute 1D root of: f(x) - target = 0
					int rootIterCount = 0;
					float a1 = t1, a2 = t2;
					for (;;)
					{
						// Use a mix of the secant rule and bisection.
						float t;
						if ((rootIterCount & 1) != 0)
						{
							// Secant rule to improve convergence.
							t = a1 + (target - s1) * (a2 - a1) / (s2 - s1);
						}
						else
						{
							// Bisection to guarantee progress.
							t = 0.5f * (a1 + a2);
						}

						++rootIterCount;

						if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
							++TOIRootIters;

						var s = SeparationFunction.Evaluate(indexA, indexB, t);

						if (Math.Abs(s - target) < tolerance)
						{
							// t2 holds a tentative value for t1
							t2 = t;
							break;
						}

						// Ensure we continue to bracket the root.
						if (s > target)
						{
							a1 = t;
							s1 = s;
						}
						else
						{
							a2 = t;
							s2 = s;
						}

						if (rootIterCount == 50)
						{
							break;
						}
					}

					if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
						TOIMaxRootIters = Math.Max(TOIMaxRootIters, rootIterCount);

					++pushBackIter;

					if (pushBackIter == Settings.MaxPolygonVertices)
						break;
				}

				++iter;

				if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
					++TOIIters;

				if (done)
					break;

				if (iter == k_maxIterations)
				{
					// Root finder got stuck. Semi-victory.
					output.State = TOIOutputState.Failed;
					output.T = t1;
					break;
				}
			}

			if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
				TOIMaxIters = Math.Max(TOIMaxIters, iter);
		}
	}
}