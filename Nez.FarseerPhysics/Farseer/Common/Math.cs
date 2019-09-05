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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common
{
	public static class MathUtils
	{
		public static float Cross(ref Vector2 a, ref Vector2 b)
		{
			return a.X * b.Y - a.Y * b.X;
		}

		public static float Cross(Vector2 a, Vector2 b)
		{
			return Cross(ref a, ref b);
		}

		/// Perform the cross product on two vectors.
		public static Vector3 Cross(Vector3 a, Vector3 b)
		{
			return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}

		public static Vector2 Cross(Vector2 a, float s)
		{
			return new Vector2(s * a.Y, -s * a.X);
		}

		public static Vector2 Cross(float s, Vector2 a)
		{
			return new Vector2(-s * a.Y, s * a.X);
		}

		public static Vector2 Abs(Vector2 v)
		{
			return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
		}

		public static Vector2 Mul(ref Mat22 A, Vector2 v)
		{
			return Mul(ref A, ref v);
		}

		public static Vector2 Mul(ref Mat22 A, ref Vector2 v)
		{
			return new Vector2(A.Ex.X * v.X + A.Ey.X * v.Y, A.Ex.Y * v.X + A.Ey.Y * v.Y);
		}

		public static Vector2 Mul(ref Transform T, Vector2 v)
		{
			return Mul(ref T, ref v);
		}

		public static Vector2 Mul(ref Transform T, ref Vector2 v)
		{
			float x = (T.Q.C * v.X - T.Q.S * v.Y) + T.P.X;
			float y = (T.Q.S * v.X + T.Q.C * v.Y) + T.P.Y;

			return new Vector2(x, y);
		}

		public static Vector2 MulT(ref Mat22 A, Vector2 v)
		{
			return MulT(ref A, ref v);
		}

		public static Vector2 MulT(ref Mat22 A, ref Vector2 v)
		{
			return new Vector2(v.X * A.Ex.X + v.Y * A.Ex.Y, v.X * A.Ey.X + v.Y * A.Ey.Y);
		}

		public static Vector2 MulT(ref Transform T, Vector2 v)
		{
			return MulT(ref T, ref v);
		}

		public static Vector2 MulT(ref Transform T, ref Vector2 v)
		{
			float px = v.X - T.P.X;
			float py = v.Y - T.P.Y;
			float x = (T.Q.C * px + T.Q.S * py);
			float y = (-T.Q.S * px + T.Q.C * py);

			return new Vector2(x, y);
		}

		// A^T * B
		public static void MulT(ref Mat22 A, ref Mat22 B, out Mat22 C)
		{
			C = new Mat22();
			C.Ex.X = A.Ex.X * B.Ex.X + A.Ex.Y * B.Ex.Y;
			C.Ex.Y = A.Ey.X * B.Ex.X + A.Ey.Y * B.Ex.Y;
			C.Ey.X = A.Ex.X * B.Ey.X + A.Ex.Y * B.Ey.Y;
			C.Ey.Y = A.Ey.X * B.Ey.X + A.Ey.Y * B.Ey.Y;
		}

		/// Multiply a matrix times a vector.
		public static Vector3 Mul(Mat33 A, Vector3 v)
		{
			return v.X * A.Ex + v.Y * A.Ey + v.Z * A.Ez;
		}

		// v2 = A.q.Rot(B.q.Rot(v1) + B.p) + A.p
		//    = (A.q * B.q).Rot(v1) + A.q.Rot(B.p) + A.p
		public static Transform Mul(Transform A, Transform B)
		{
			Transform C = new Transform();
			C.Q = Mul(A.Q, B.Q);
			C.P = Mul(A.Q, B.P) + A.P;
			return C;
		}

		// v2 = A.q' * (B.q * v1 + B.p - A.p)
		//    = A.q' * B.q * v1 + A.q' * (B.p - A.p)
		public static void MulT(ref Transform A, ref Transform B, out Transform C)
		{
			C = new Transform();
			C.Q = MulT(A.Q, B.Q);
			C.P = MulT(A.Q, B.P - A.P);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}

		/// Multiply a matrix times a vector.
		public static Vector2 Mul22(Mat33 A, Vector2 v)
		{
			return new Vector2(A.Ex.X * v.X + A.Ey.X * v.Y, A.Ex.Y * v.X + A.Ey.Y * v.Y);
		}

		/// Multiply two rotations: q * r
		public static Rot Mul(Rot q, Rot r)
		{
			// [qc -qs] * [rc -rs] = [qc*rc-qs*rs -qc*rs-qs*rc]
			// [qs  qc]   [rs  rc]   [qs*rc+qc*rs -qs*rs+qc*rc]
			// s = qs * rc + qc * rs
			// c = qc * rc - qs * rs
			Rot qr;
			qr.S = q.S * r.C + q.C * r.S;
			qr.C = q.C * r.C - q.S * r.S;
			return qr;
		}

		public static Vector2 MulT(Transform T, Vector2 v)
		{
			float px = v.X - T.P.X;
			float py = v.Y - T.P.Y;
			float x = (T.Q.C * px + T.Q.S * py);
			float y = (-T.Q.S * px + T.Q.C * py);

			return new Vector2(x, y);
		}

		/// Transpose multiply two rotations: qT * r
		public static Rot MulT(Rot q, Rot r)
		{
			// [ qc qs] * [rc -rs] = [qc*rc+qs*rs -qc*rs+qs*rc]
			// [-qs qc]   [rs  rc]   [-qs*rc+qc*rs qs*rs+qc*rc]
			// s = qc * rs - qs * rc
			// c = qc * rc + qs * rs
			Rot qr;
			qr.S = q.C * r.S - q.S * r.C;
			qr.C = q.C * r.C + q.S * r.S;
			return qr;
		}

		// v2 = A.q' * (B.q * v1 + B.p - A.p)
		//    = A.q' * B.q * v1 + A.q' * (B.p - A.p)
		public static Transform MulT(Transform A, Transform B)
		{
			var C = new Transform();
			C.Q = MulT(A.Q, B.Q);
			C.P = MulT(A.Q, B.P - A.P);
			return C;
		}

		/// Rotate a vector
		public static Vector2 Mul(Rot q, Vector2 v)
		{
			return new Vector2(q.C * v.X - q.S * v.Y, q.S * v.X + q.C * v.Y);
		}

		/// Inverse rotate a vector
		public static Vector2 MulT(Rot q, Vector2 v)
		{
			return new Vector2(q.C * v.X + q.S * v.Y, -q.S * v.X + q.C * v.Y);
		}

		/// Get the skew vector such that dot(skew_vec, other) == cross(vec, other)
		public static Vector2 Skew(Vector2 input)
		{
			return new Vector2(-input.Y, input.X);
		}

		/// <summary>
		/// This function is used to ensure that a floating point number is
		/// not a NaN or infinity.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns>
		/// 	<c>true</c> if the specified x is valid; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValid(float x)
		{
			if (float.IsNaN(x))
			{
				// NaN.
				return false;
			}

			return !float.IsInfinity(x);
		}

		public static bool IsValid(this Vector2 x)
		{
			return IsValid(x.X) && IsValid(x.Y);
		}

		/// <summary>
		/// This is a approximate yet fast inverse square-root.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns></returns>
		public static float InvSqrt(float x)
		{
			FloatConverter convert = new FloatConverter();
			convert.X = x;
			float xhalf = 0.5f * x;
			convert.I = 0x5f3759df - (convert.I >> 1);
			x = convert.X;
			x = x * (1.5f - xhalf * x * x);
			return x;
		}

		public static int Clamp(int a, int low, int high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static float Clamp(float a, float low, float high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
		{
			return Vector2.Max(low, Vector2.Min(a, high));
		}

		public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
		{
			c = a.X * b.Y - a.Y * b.X;
		}

		/// <summary>
		/// Return the angle between two vectors on a plane
		/// The angle is from vector 1 to vector 2, positive anticlockwise
		/// The result is between -pi -> pi
		/// </summary>
		public static double VectorAngle(ref Vector2 p1, ref Vector2 p2)
		{
			double theta1 = Math.Atan2(p1.Y, p1.X);
			double theta2 = Math.Atan2(p2.Y, p2.X);
			double dtheta = theta2 - theta1;
			while (dtheta > Math.PI)
				dtheta -= (2 * Math.PI);
			while (dtheta < -Math.PI)
				dtheta += (2 * Math.PI);

			return (dtheta);
		}

		/// Perform the dot product on two vectors.
		public static float Dot(Vector3 a, Vector3 b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		public static double VectorAngle(Vector2 p1, Vector2 p2)
		{
			return VectorAngle(ref p1, ref p2);
		}

		/// <summary>
		/// Returns a positive number if c is to the left of the line going from a to b.
		/// </summary>
		/// <returns>Positive number if point is left, negative if point is right, 
		/// and 0 if points are collinear.</returns>
		public static float Area(Vector2 a, Vector2 b, Vector2 c)
		{
			return Area(ref a, ref b, ref c);
		}

		/// <summary>
		/// Returns a positive number if c is to the left of the line going from a to b.
		/// </summary>
		/// <returns>Positive number if point is left, negative if point is right, 
		/// and 0 if points are collinear.</returns>
		public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
		}

		/// <summary>
		/// Determines if three vertices are collinear (ie. on a straight line)
		/// </summary>
		/// <param name="a">First vertex</param>
		/// <param name="b">Second vertex</param>
		/// <param name="c">Third vertex</param>
		/// <param name="tolerance">The tolerance</param>
		/// <returns></returns>
		public static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0)
		{
			return FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
		}

		public static void Cross(float s, ref Vector2 a, out Vector2 b)
		{
			b = new Vector2(-s * a.Y, s * a.X);
		}

		public static bool FloatEquals(float value1, float value2)
		{
			return Math.Abs(value1 - value2) <= Settings.Epsilon;
		}

		/// <summary>
		/// Checks if a floating point Value is equal to another,
		/// within a certain tolerance.
		/// </summary>
		/// <param name="value1">The first floating point Value.</param>
		/// <param name="value2">The second floating point Value.</param>
		/// <param name="delta">The floating point tolerance.</param>
		/// <returns>True if the values are "equal", false otherwise.</returns>
		public static bool FloatEquals(float value1, float value2, float delta)
		{
			return FloatInRange(value1, value2 - delta, value2 + delta);
		}

		/// <summary>
		/// Checks if a floating point Value is within a specified
		/// range of values (inclusive).
		/// </summary>
		/// <param name="value">The Value to check.</param>
		/// <param name="min">The minimum Value.</param>
		/// <param name="max">The maximum Value.</param>
		/// <returns>True if the Value is within the range specified,
		/// false otherwise.</returns>
		public static bool FloatInRange(float value, float min, float max)
		{
			return (value >= min && value <= max);
		}


		#region Nested type: FloatConverter

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatConverter
		{
			[FieldOffset(0)] public float X;
			[FieldOffset(0)] public int I;
		}

		#endregion


		public static Vector2 Mul(ref Rot rot, Vector2 axis)
		{
			return Mul(rot, axis);
		}

		public static Vector2 MulT(ref Rot rot, Vector2 axis)
		{
			return MulT(rot, axis);
		}
	}


	/// <summary>
	/// A 2-by-2 matrix. Stored in column-major order.
	/// </summary>
	public struct Mat22
	{
		public Vector2 Ex, Ey;

		/// <summary>
		/// Construct this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		public Mat22(Vector2 c1, Vector2 c2)
		{
			Ex = c1;
			Ey = c2;
		}

		/// <summary>
		/// Construct this matrix using scalars.
		/// </summary>
		/// <param name="a11">The a11.</param>
		/// <param name="a12">The a12.</param>
		/// <param name="a21">The a21.</param>
		/// <param name="a22">The a22.</param>
		public Mat22(float a11, float a12, float a21, float a22)
		{
			Ex = new Vector2(a11, a21);
			Ey = new Vector2(a12, a22);
		}

		public Mat22 Inverse
		{
			get
			{
				float a = Ex.X, b = Ey.X, c = Ex.Y, d = Ey.Y;
				float det = a * d - b * c;
				if (det != 0.0f)
				{
					det = 1.0f / det;
				}

				Mat22 result = new Mat22();
				result.Ex.X = det * d;
				result.Ex.Y = -det * c;

				result.Ey.X = -det * b;
				result.Ey.Y = det * a;

				return result;
			}
		}

		/// <summary>
		/// Initialize this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		public void Set(Vector2 c1, Vector2 c2)
		{
			Ex = c1;
			Ey = c2;
		}

		/// <summary>
		/// Set this to the identity matrix.
		/// </summary>
		public void SetIdentity()
		{
			Ex.X = 1.0f;
			Ey.X = 0.0f;
			Ex.Y = 0.0f;
			Ey.Y = 1.0f;
		}

		/// <summary>
		/// Set this matrix to all zeros.
		/// </summary>
		public void SetZero()
		{
			Ex.X = 0.0f;
			Ey.X = 0.0f;
			Ex.Y = 0.0f;
			Ey.Y = 0.0f;
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector2 Solve(Vector2 b)
		{
			float a11 = Ex.X, a12 = Ey.X, a21 = Ex.Y, a22 = Ey.Y;
			float det = a11 * a22 - a12 * a21;
			if (det != 0.0f)
			{
				det = 1.0f / det;
			}

			return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
		}

		public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
		{
			R.Ex = A.Ex + B.Ex;
			R.Ey = A.Ey + B.Ey;
		}
	}


	/// <summary>
	/// A 3-by-3 matrix. Stored in column-major order.
	/// </summary>
	public struct Mat33
	{
		public Vector3 Ex, Ey, Ez;

		/// <summary>
		/// Construct this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		/// <param name="c3">The c3.</param>
		public Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
		{
			Ex = c1;
			Ey = c2;
			Ez = c3;
		}

		/// <summary>
		/// Set this matrix to all zeros.
		/// </summary>
		public void SetZero()
		{
			Ex = Vector3.Zero;
			Ey = Vector3.Zero;
			Ez = Vector3.Zero;
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector3 Solve33(Vector3 b)
		{
			float det = Vector3.Dot(Ex, Vector3.Cross(Ey, Ez));
			if (det != 0.0f)
			{
				det = 1.0f / det;
			}

			return new Vector3(det * Vector3.Dot(b, Vector3.Cross(Ey, Ez)), det * Vector3.Dot(Ex, Vector3.Cross(b, Ez)),
				det * Vector3.Dot(Ex, Vector3.Cross(Ey, b)));
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases. Solve only the upper
		/// 2-by-2 matrix equation.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector2 Solve22(Vector2 b)
		{
			float a11 = Ex.X, a12 = Ey.X, a21 = Ex.Y, a22 = Ey.Y;
			float det = a11 * a22 - a12 * a21;

			if (det != 0.0f)
			{
				det = 1.0f / det;
			}

			return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
		}

		/// Get the inverse of this matrix as a 2-by-2.
		/// Returns the zero matrix if singular.
		public void GetInverse22(ref Mat33 M)
		{
			float a = Ex.X, b = Ey.X, c = Ex.Y, d = Ey.Y;
			float det = a * d - b * c;
			if (det != 0.0f)
			{
				det = 1.0f / det;
			}

			M.Ex.X = det * d;
			M.Ey.X = -det * b;
			M.Ex.Z = 0.0f;
			M.Ex.Y = -det * c;
			M.Ey.Y = det * a;
			M.Ey.Z = 0.0f;
			M.Ez.X = 0.0f;
			M.Ez.Y = 0.0f;
			M.Ez.Z = 0.0f;
		}

		/// Get the symmetric inverse of this matrix as a 3-by-3.
		/// Returns the zero matrix if singular.
		public void GetSymInverse33(ref Mat33 M)
		{
			float det = MathUtils.Dot(Ex, MathUtils.Cross(Ey, Ez));
			if (det != 0.0f)
			{
				det = 1.0f / det;
			}

			float a11 = Ex.X, a12 = Ey.X, a13 = Ez.X;
			float a22 = Ey.Y, a23 = Ez.Y;
			float a33 = Ez.Z;

			M.Ex.X = det * (a22 * a33 - a23 * a23);
			M.Ex.Y = det * (a13 * a23 - a12 * a33);
			M.Ex.Z = det * (a12 * a23 - a13 * a22);

			M.Ey.X = M.Ex.Y;
			M.Ey.Y = det * (a11 * a33 - a13 * a13);
			M.Ey.Z = det * (a13 * a12 - a11 * a23);

			M.Ez.X = M.Ex.Z;
			M.Ez.Y = M.Ey.Z;
			M.Ez.Z = det * (a11 * a22 - a12 * a12);
		}
	}


	/// <summary>
	/// Rotation
	/// </summary>
	public struct Rot
	{
		/// Sine and cosine
		public float S, C;

		/// <summary>
		/// Initialize from an angle in radians
		/// </summary>
		/// <param name="angle">Angle in radians</param>
		public Rot(float angle)
		{
			// TODO_ERIN optimize
			S = (float) Math.Sin(angle);
			C = (float) Math.Cos(angle);
		}

		/// <summary>
		/// Set using an angle in radians.
		/// </summary>
		/// <param name="angle"></param>
		public void Set(float angle)
		{
			//FPE: Optimization
			if (angle == 0)
			{
				S = 0;
				C = 1;
			}
			else
			{
				// TODO_ERIN optimize
				S = (float) Math.Sin(angle);
				C = (float) Math.Cos(angle);
			}
		}

		/// <summary>
		/// Set to the identity rotation
		/// </summary>
		public void SetIdentity()
		{
			S = 0.0f;
			C = 1.0f;
		}

		/// <summary>
		/// Get the angle in radians
		/// </summary>
		public float GetAngle()
		{
			return (float) Math.Atan2(S, C);
		}

		/// <summary>
		/// Get the x-axis
		/// </summary>
		public Vector2 GetXAxis()
		{
			return new Vector2(C, S);
		}

		/// <summary>
		/// Get the y-axis
		/// </summary>
		public Vector2 GetYAxis()
		{
			return new Vector2(-S, C);
		}
	}


	/// <summary>
	/// A transform contains translation and rotation. It is used to represent
	/// the position and orientation of rigid frames.
	/// </summary>
	public struct Transform
	{
		public Vector2 P;
		public Rot Q;

		/// <summary>
		/// Initialize using a position vector and a rotation matrix.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="rotation">The r.</param>
		public Transform(ref Vector2 position, ref Rot rotation)
		{
			P = position;
			Q = rotation;
		}

		/// <summary>
		/// Set this to the identity transform.
		/// </summary>
		public void SetIdentity()
		{
			P = Vector2.Zero;
			Q.SetIdentity();
		}

		/// <summary>
		/// Set this based on the position and angle.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="angle">The angle.</param>
		public void Set(Vector2 position, float angle)
		{
			P = position;
			Q.Set(angle);
		}
	}


	/// <summary>
	/// This describes the motion of a body/shape for TOI computation.
	/// Shapes are defined with respect to the body origin, which may
	/// no coincide with the center of mass. However, to support dynamics
	/// we must interpolate the center of mass position.
	/// </summary>
	public struct Sweep
	{
		/// <summary>
		/// World angles
		/// </summary>
		public float A;

		public float A0;

		/// <summary>
		/// Fraction of the current time step in the range [0,1]
		/// c0 and a0 are the positions at alpha0.
		/// </summary>
		public float Alpha0;

		/// <summary>
		/// Center world positions
		/// </summary>
		public Vector2 C;

		public Vector2 C0;

		/// <summary>
		/// Local center of mass position
		/// </summary>
		public Vector2 LocalCenter;


		/// <summary>
		/// Get the interpolated transform at a specific time.
		/// </summary>
		/// <param name="xfb">The transform.</param>
		/// <param name="beta">beta is a factor in [0,1], where 0 indicates alpha0.</param>
		public void GetTransform(out Transform xfb, float beta)
		{
			xfb = new Transform();
			xfb.P.X = (1.0f - beta) * C0.X + beta * C.X;
			xfb.P.Y = (1.0f - beta) * C0.Y + beta * C.Y;
			var angle = (1.0f - beta) * A0 + beta * A;
			xfb.Q.Set(angle);

			// Shift to origin
			xfb.P -= MathUtils.Mul(xfb.Q, LocalCenter);
		}

		/// <summary>
		/// Advance the sweep forward, yielding a new initial state.
		/// </summary>
		/// <param name="alpha">new initial time..</param>
		public void Advance(float alpha)
		{
			Debug.Assert(Alpha0 < 1.0f);
			var beta = (alpha - Alpha0) / (1.0f - Alpha0);
			C0 += beta * (C - C0);
			A0 += beta * (A - A0);
			Alpha0 = alpha;
		}

		/// <summary>
		/// Normalize the angles.
		/// </summary>
		public void Normalize()
		{
			var d = MathHelper.TwoPi * (float) Math.Floor(A0 / MathHelper.TwoPi);
			A0 -= d;
			A -= d;
		}
	}
}