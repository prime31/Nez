using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// A type of body that supports multiple fixtures that can break apart.
	/// </summary>
	public class BreakableBody
	{
		public bool IsBroken;
		public Body MainBody;
		public List<Fixture> Parts = new List<Fixture>(8);

		/// <summary>
		/// The force needed to break the body apart.
		/// Default: 500
		/// </summary>
		public float Strength = 500.0f;

		float[] _angularVelocitiesCache = new float[8];
		bool _break;
		Vector2[] _velocitiesCache = new Vector2[8];
		World _world;


		public BreakableBody(World world, IEnumerable<Vertices> vertices, float density,
		                     Vector2 position = new Vector2(), float rotation = 0)
		{
			_world = world;
			_world.ContactManager.OnPostSolve += OnPostSolve;
			MainBody = new Body(_world, position, rotation, BodyType.Dynamic);

			foreach (Vertices part in vertices)
			{
				var polygonShape = new PolygonShape(part, density);
				Fixture fixture = MainBody.CreateFixture(polygonShape);
				Parts.Add(fixture);
			}
		}

		public BreakableBody(World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(),
		                     float rotation = 0)
		{
			_world = world;
			_world.ContactManager.OnPostSolve += OnPostSolve;
			MainBody = new Body(_world, position, rotation, BodyType.Dynamic);

			foreach (Shape part in shapes)
			{
				Fixture fixture = MainBody.CreateFixture(part);
				Parts.Add(fixture);
			}
		}

		void OnPostSolve(Contact contact, ContactVelocityConstraint impulse)
		{
			if (!IsBroken)
			{
				if (Parts.Contains(contact.FixtureA) || Parts.Contains(contact.FixtureB))
				{
					float maxImpulse = 0.0f;
					int count = contact.Manifold.PointCount;

					for (int i = 0; i < count; ++i)
					{
						maxImpulse = Math.Max(maxImpulse, impulse.Points[i].NormalImpulse);
					}

					if (maxImpulse > Strength)
					{
						// Flag the body for breaking.
						_break = true;
					}
				}
			}
		}

		public void Update()
		{
			if (_break)
			{
				Decompose();
				IsBroken = true;
				_break = false;
			}

			// Cache velocities to improve movement on breakage.
			if (IsBroken == false)
			{
				//Enlarge the cache if needed
				if (Parts.Count > _angularVelocitiesCache.Length)
				{
					_velocitiesCache = new Vector2[Parts.Count];
					_angularVelocitiesCache = new float[Parts.Count];
				}

				//Cache the linear and angular velocities.
				for (int i = 0; i < Parts.Count; i++)
				{
					_velocitiesCache[i] = Parts[i].Body.LinearVelocity;
					_angularVelocitiesCache[i] = Parts[i].Body.AngularVelocity;
				}
			}
		}

		void Decompose()
		{
			// Unsubsribe from the PostSolve delegate
			_world.ContactManager.OnPostSolve -= OnPostSolve;

			for (int i = 0; i < Parts.Count; i++)
			{
				var oldFixture = Parts[i];

				var shape = oldFixture.Shape.Clone();
				object userData = oldFixture.UserData;

				MainBody.DestroyFixture(oldFixture);

				var body = BodyFactory.CreateBody(_world, MainBody.Position, MainBody.Rotation, BodyType.Dynamic,
					MainBody.UserData);

				var newFixture = body.CreateFixture(shape);
				newFixture.UserData = userData;
				Parts[i] = newFixture;

				body.AngularVelocity = _angularVelocitiesCache[i];
				body.LinearVelocity = _velocitiesCache[i];
			}

			_world.RemoveBody(MainBody);
			_world.RemoveBreakableBody(this);
		}

		public void BreakBody()
		{
			_break = true;
		}
	}
}