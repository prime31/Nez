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
		public float MinRadius;
		public float MaxRadius;
		public float Strength;
		public GravityType GravityType = GravityType.DistanceSquared;
		public List<Body> Bodies = new List<Body>();
		public List<Vector2> Points = new List<Vector2>();


		public GravityController(float strength) : base(ControllerType.GravityController)
		{
			this.Strength = strength;
			MaxRadius = float.MaxValue;
		}

		public GravityController(float strength, float maxRadius, float minRadius) : base(ControllerType
			.GravityController)
		{
			this.MinRadius = minRadius;
			this.MaxRadius = maxRadius;
			this.Strength = strength;
			this.GravityType = GravityType.DistanceSquared;
			this.Points = new List<Vector2>();
			this.Bodies = new List<Body>();
		}

		public override void Update(float dt)
		{
			var f = Vector2.Zero;

			foreach (var worldBody in World.BodyList)
			{
				if (!IsActiveOn(worldBody))
					continue;

				foreach (Body controllerBody in Bodies)
				{
					if (worldBody == controllerBody || (worldBody.IsStatic && controllerBody.IsStatic) ||
					    !controllerBody.Enabled)
						continue;

					var d = controllerBody.Position - worldBody.Position;
					var r2 = d.LengthSquared();

					if (r2 <= Settings.Epsilon || r2 > MaxRadius * MaxRadius || r2 < MinRadius * MinRadius)
						continue;

					switch (GravityType)
					{
						case GravityType.DistanceSquared:
							f = Strength / r2 * worldBody.Mass * controllerBody.Mass * d;
							break;
						case GravityType.Linear:
							f = Strength / (float) Math.Sqrt(r2) * worldBody.Mass * controllerBody.Mass * d;
							break;
					}

					worldBody.ApplyForce(ref f);
				}

				foreach (Vector2 point in Points)
				{
					var d = point - worldBody.Position;
					var r2 = d.LengthSquared();

					if (r2 <= Settings.Epsilon || r2 > MaxRadius * MaxRadius || r2 < MinRadius * MinRadius)
						continue;

					switch (GravityType)
					{
						case GravityType.DistanceSquared:
							f = Strength / r2 * worldBody.Mass * d;
							break;
						case GravityType.Linear:
							f = Strength / (float) Math.Sqrt(r2) * worldBody.Mass * d;
							break;
					}

					worldBody.ApplyForce(ref f);
				}
			}
		}

		public void AddBody(Body body)
		{
			Bodies.Add(body);
		}

		public void AddPoint(Vector2 point)
		{
			Points.Add(point);
		}
	}
}