using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Controllers
{
	public enum GravityType
	{
		Linear,
		DistanceSquared
	}


	public class GravityController : Controller
	{
		public float minRadius;
		public float maxRadius;
		public float strength;
		public GravityType gravityType = GravityType.DistanceSquared;
		public List<Body> bodies = new List<Body>();
		public List<Vector2> points = new List<Vector2>();


		public GravityController( float strength ) : base( ControllerType.GravityController )
		{
			this.strength = strength;
			maxRadius = float.MaxValue;
		}

		public GravityController( float strength, float maxRadius, float minRadius ) : base( ControllerType.GravityController )
		{
			this.minRadius = minRadius;
			this.maxRadius = maxRadius;
			this.strength = strength;
			this.gravityType = GravityType.DistanceSquared;
			this.points = new List<Vector2>();
			this.bodies = new List<Body>();
		}

		public override void update( float dt )
		{
			var f = Vector2.Zero;

			foreach( var worldBody in world.bodyList )
			{
				if( !isActiveOn( worldBody ) )
					continue;

				foreach( Body controllerBody in bodies )
				{
					if( worldBody == controllerBody || ( worldBody.isStatic && controllerBody.isStatic ) || !controllerBody.enabled )
						continue;

					var d = controllerBody.position - worldBody.position;
					var r2 = d.LengthSquared();

					if( r2 <= Settings.epsilon || r2 > maxRadius * maxRadius || r2 < minRadius * minRadius )
						continue;

					switch( gravityType )
					{
						case GravityType.DistanceSquared:
							f = strength / r2 * worldBody.mass * controllerBody.mass * d;
							break;
						case GravityType.Linear:
							f = strength / (float)Math.Sqrt( r2 ) * worldBody.mass * controllerBody.mass * d;
							break;
					}

					worldBody.applyForce( ref f );
				}

				foreach( Vector2 point in points )
				{
					var d = point - worldBody.position;
					var r2 = d.LengthSquared();

					if( r2 <= Settings.epsilon || r2 > maxRadius * maxRadius || r2 < minRadius * minRadius )
						continue;

					switch( gravityType )
					{
						case GravityType.DistanceSquared:
							f = strength / r2 * worldBody.mass * d;
							break;
						case GravityType.Linear:
							f = strength / (float)Math.Sqrt( r2 ) * worldBody.mass * d;
							break;
					}

					worldBody.applyForce( ref f );
				}
			}
		}

		public void addBody( Body body )
		{
			bodies.Add( body );
		}

		public void addPoint( Vector2 point )
		{
			points.Add( point );
		}

	}
}