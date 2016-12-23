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


namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// A circle shape.
	/// </summary>
	public class CircleShape : Shape
	{
		public override int childCount
		{
			get { return 1; }
		}

		/// <summary>
		/// Get or set the position of the circle
		/// </summary>
		public Vector2 position
		{
			get { return _position; }
			set
			{
				_position = value;
				computeProperties(); //TODO: Optimize here
			}
		}

		internal Vector2 _position;


		/// <summary>
		/// Create a new circle with the desired radius and density.
		/// </summary>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="density">The density of the circle.</param>
		public CircleShape( float radius, float density ) : base( density )
		{
			Debug.Assert( radius >= 0 );
			Debug.Assert( density >= 0 );

			shapeType = ShapeType.Circle;
			_position = Vector2.Zero;
			base.radius = radius; // The Radius property cache 2radius and calls ComputeProperties(). So no need to call ComputeProperties() here.
		}

		internal CircleShape() : base( 0 )
		{
			shapeType = ShapeType.Circle;
			_radius = 0.0f;
			_position = Vector2.Zero;
		}

		public override bool testPoint( ref Transform transform, ref Vector2 point )
		{
			var center = transform.p + MathUtils.mul( transform.q, position );
			var d = point - center;
			return Vector2.Dot( d, d ) <= _2radius;
		}

		public override bool rayCast( out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex )
		{
			// Collision Detection in Interactive 3D Environments by Gino van den Bergen
			// From Section 3.1.2
			// x = s + a * r
			// norm(x) = radius

			output = new RayCastOutput();

			var pos = transform.p + MathUtils.mul( transform.q, this.position );
			var s = input.point1 - pos;
			var b = Vector2.Dot( s, s ) - _2radius;

			// Solve quadratic equation.
			var r = input.point2 - input.point1;
			var c = Vector2.Dot( s, r );
			var rr = Vector2.Dot( r, r );
			var sigma = c * c - rr * b;

			// Check for negative discriminant and short segment.
			if( sigma < 0.0f || rr < Settings.epsilon )
				return false;

			// Find the point of intersection of the line with the circle.
			float a = -( c + (float)Math.Sqrt( sigma ) );

			// Is the intersection point on the segment?
			if( 0.0f <= a && a <= input.maxFraction * rr )
			{
				a /= rr;
				output.fraction = a;

				//TODO: Check results here
				output.normal = s + a * r;
				Nez.Vector2Ext.normalize( ref output.normal );
				return true;
			}

			return false;
		}

		public override void computeAABB( out AABB aabb, ref Transform transform, int childIndex )
		{
			var p = transform.p + MathUtils.mul( transform.q, position );
			aabb.lowerBound = new Vector2( p.X - radius, p.Y - radius );
			aabb.upperBound = new Vector2( p.X + radius, p.Y + radius );
		}

		protected override sealed void computeProperties()
		{
			var area = Settings.pi * _2radius;
			massData.area = area;
			massData.mass = density * area;
			massData.centroid = position;

			// inertia about the local origin
			massData.inertia = massData.mass * ( 0.5f * _2radius + Vector2.Dot( position, position ) );
		}

		public override float computeSubmergedArea( ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc )
		{
			sc = Vector2.Zero;

			var p = MathUtils.mul( ref xf, position );
			float l = -( Vector2.Dot( normal, p ) - offset );
			if( l < -radius + Settings.epsilon )
			{
				//Completely dry
				return 0;
			}
			if( l > radius )
			{
				//Completely wet
				sc = p;
				return Settings.pi * _2radius;
			}

			//Magic
			float l2 = l * l;
			float area = _2radius * (float)( ( Math.Asin( l / radius ) + Settings.pi / 2 ) + l * Math.Sqrt( _2radius - l2 ) );
			float com = -2.0f / 3.0f * (float)Math.Pow( _2radius - l2, 1.5f ) / area;

			sc.X = p.X + normal.X * com;
			sc.Y = p.Y + normal.Y * com;

			return area;
		}

		/// <summary>
		/// Compare the circle to another circle
		/// </summary>
		/// <param name="shape">The other circle</param>
		/// <returns>True if the two circles are the same size and have the same position</returns>
		public bool CompareTo( CircleShape shape )
		{
			return ( radius == shape.radius && position == shape.position );
		}

		public override Shape clone()
		{
			CircleShape clone = new CircleShape();
			clone.shapeType = shapeType;
			clone._radius = radius;
			clone._2radius = _2radius; //FPE note: We also copy the cache
			clone._density = _density;
			clone._position = _position;
			clone.massData = massData;
			return clone;
		}

	}
}