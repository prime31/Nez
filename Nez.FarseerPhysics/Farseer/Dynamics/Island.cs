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
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// This is an internal class.
	/// </summary>
	public class Island
	{
		public Body[] Bodies;
		public int BodyCount;
		public int ContactCount;
		public int JointCount;

		public Velocity[] _velocities;
		public Position[] _positions;

		public int BodyCapacity;
		public int ContactCapacity;
		public int JointCapacity;
		public float JointUpdateTime;

		ContactManager _contactManager;
		ContactSolver _contactSolver = new ContactSolver();
		Contact[] _contacts;
		Joint[] _joints;

		const float LinTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
		const float AngTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;
		Stopwatch _watch = new Stopwatch();


		public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
		{
			BodyCapacity = bodyCapacity;
			ContactCapacity = contactCapacity;
			JointCapacity = jointCapacity;
			BodyCount = 0;
			ContactCount = 0;
			JointCount = 0;

			_contactManager = contactManager;

			if (Bodies == null || Bodies.Length < bodyCapacity)
			{
				Bodies = new Body[bodyCapacity];
				_velocities = new Velocity[bodyCapacity];
				_positions = new Position[bodyCapacity];
			}

			if (_contacts == null || _contacts.Length < contactCapacity)
			{
				_contacts = new Contact[contactCapacity * 2];
			}

			if (_joints == null || _joints.Length < jointCapacity)
			{
				_joints = new Joint[jointCapacity * 2];
			}
		}

		public void Clear()
		{
			BodyCount = 0;
			ContactCount = 0;
			JointCount = 0;
		}

		public void Solve(ref TimeStep step, ref Vector2 gravity)
		{
			float h = step.Dt;

			// Integrate velocities and apply damping. Initialize the body state.
			for (int i = 0; i < BodyCount; ++i)
			{
				var b = Bodies[i];

				var c = b._sweep.C;
				float a = b._sweep.A;
				var v = b._linearVelocity;
				float w = b._angularVelocity;

				// Store positions for continuous collision.
				b._sweep.C0 = b._sweep.C;
				b._sweep.A0 = b._sweep.A;

				if (b.BodyType == BodyType.Dynamic)
				{
					// Integrate velocities.

					// FPE: Only apply gravity if the body wants it.
					if (b.IgnoreGravity)
						v += h * (b._invMass * b._force);
					else
						v += h * (b.GravityScale * gravity + b._invMass * b._force);

					w += h * b._invI * b._torque;

					// Apply damping.
					// ODE: dv/dt + c * v = 0
					// Solution: v(t) = v0 * exp(-c * t)
					// Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
					// v2 = exp(-c * dt) * v1
					// Taylor expansion:
					// v2 = (1.0f - c * dt) * v1
					v *= MathUtils.Clamp(1.0f - h * b.LinearDamping, 0.0f, 1.0f);
					w *= MathUtils.Clamp(1.0f - h * b.AngularDamping, 0.0f, 1.0f);
				}

				_positions[i].C = c;
				_positions[i].A = a;
				_velocities[i].V = v;
				_velocities[i].W = w;
			}

			// Solver data
			SolverData solverData = new SolverData();
			solverData.Step = step;
			solverData.Positions = _positions;
			solverData.Velocities = _velocities;

			_contactSolver.Reset(step, ContactCount, _contacts, _positions, _velocities);
			_contactSolver.InitializeVelocityConstraints();

			if (Settings.EnableWarmstarting)
			{
				_contactSolver.WarmStart();
			}

			if (Settings.EnableDiagnostics)
				_watch.Start();

			for (int i = 0; i < JointCount; ++i)
			{
				if (_joints[i].Enabled)
					_joints[i].InitVelocityConstraints(ref solverData);
			}

			if (Settings.EnableDiagnostics)
				_watch.Stop();

			// Solve velocity constraints.
			for (int i = 0; i < Settings.VelocityIterations; ++i)
			{
				for (int j = 0; j < JointCount; ++j)
				{
					Joint joint = _joints[j];

					if (!joint.Enabled)
						continue;

					if (Settings.EnableDiagnostics)
						_watch.Start();

					joint.SolveVelocityConstraints(ref solverData);
					joint.Validate(step.Inv_dt);

					if (Settings.EnableDiagnostics)
						_watch.Stop();
				}

				_contactSolver.SolveVelocityConstraints();
			}

			// Store impulses for warm starting.
			_contactSolver.StoreImpulses();

			// Integrate positions
			for (int i = 0; i < BodyCount; ++i)
			{
				Vector2 c = _positions[i].C;
				float a = _positions[i].A;
				Vector2 v = _velocities[i].V;
				float w = _velocities[i].W;

				// Check for large velocities
				Vector2 translation = h * v;
				if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
				{
					float ratio = Settings.MaxTranslation / translation.Length();
					v *= ratio;
				}

				float rotation = h * w;
				if (rotation * rotation > Settings.MaxRotationSquared)
				{
					float ratio = Settings.MaxRotation / Math.Abs(rotation);
					w *= ratio;
				}

				// Integrate
				c += h * v;
				a += h * w;

				_positions[i].C = c;
				_positions[i].A = a;
				_velocities[i].V = v;
				_velocities[i].W = w;
			}


			// Solve position constraints
			bool positionSolved = false;
			for (int i = 0; i < Settings.PositionIterations; ++i)
			{
				bool contactsOkay = _contactSolver.SolvePositionConstraints();

				bool jointsOkay = true;
				for (int j = 0; j < JointCount; ++j)
				{
					Joint joint = _joints[j];

					if (!joint.Enabled)
						continue;

					if (Settings.EnableDiagnostics)
						_watch.Start();

					bool jointOkay = joint.SolvePositionConstraints(ref solverData);

					if (Settings.EnableDiagnostics)
						_watch.Stop();

					jointsOkay = jointsOkay && jointOkay;
				}

				if (contactsOkay && jointsOkay)
				{
					// Exit early if the position errors are small.
					positionSolved = true;
					break;
				}
			}

			if (Settings.EnableDiagnostics)
			{
				JointUpdateTime = _watch.ElapsedTicks;
				_watch.Reset();
			}

			// Copy state buffers back to the bodies
			for (int i = 0; i < BodyCount; ++i)
			{
				Body body = Bodies[i];
				body._sweep.C = _positions[i].C;
				body._sweep.A = _positions[i].A;
				body._linearVelocity = _velocities[i].V;
				body._angularVelocity = _velocities[i].W;
				body.SynchronizeTransform();
			}

			Report(_contactSolver._velocityConstraints);

			if (Settings.AllowSleep)
			{
				float minSleepTime = Settings.MaxFloat;

				for (int i = 0; i < BodyCount; ++i)
				{
					Body b = Bodies[i];

					if (b.BodyType == BodyType.Static)
						continue;

					if (!b.IsSleepingAllowed || b._angularVelocity * b._angularVelocity > AngTolSqr ||
					    Vector2.Dot(b._linearVelocity, b._linearVelocity) > LinTolSqr)
					{
						b._sleepTime = 0.0f;
						minSleepTime = 0.0f;
					}
					else
					{
						b._sleepTime += h;
						minSleepTime = Math.Min(minSleepTime, b._sleepTime);
					}
				}

				if (minSleepTime >= Settings.TimeToSleep && positionSolved)
				{
					for (int i = 0; i < BodyCount; ++i)
					{
						Body b = Bodies[i];
						b.IsAwake = false;
					}
				}
			}
		}

		internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB)
		{
			Debug.Assert(toiIndexA < BodyCount);
			Debug.Assert(toiIndexB < BodyCount);

			// Initialize the body state.
			for (int i = 0; i < BodyCount; ++i)
			{
				Body b = Bodies[i];
				_positions[i].C = b._sweep.C;
				_positions[i].A = b._sweep.A;
				_velocities[i].V = b._linearVelocity;
				_velocities[i].W = b._angularVelocity;
			}

			_contactSolver.Reset(subStep, ContactCount, _contacts, _positions, _velocities);

			// Solve position constraints.
			for (int i = 0; i < Settings.ToiPositionIterations; ++i)
			{
				bool contactsOkay = _contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB);
				if (contactsOkay)
				{
					break;
				}
			}

			// Leap of faith to new safe state.
			Bodies[toiIndexA]._sweep.C0 = _positions[toiIndexA].C;
			Bodies[toiIndexA]._sweep.A0 = _positions[toiIndexA].A;
			Bodies[toiIndexB]._sweep.C0 = _positions[toiIndexB].C;
			Bodies[toiIndexB]._sweep.A0 = _positions[toiIndexB].A;

			// No warm starting is needed for TOI events because warm
			// starting impulses were applied in the discrete solver.
			_contactSolver.InitializeVelocityConstraints();

			// Solve velocity constraints.
			for (int i = 0; i < Settings.ToiVelocityIterations; ++i)
			{
				_contactSolver.SolveVelocityConstraints();
			}

			// Don't store the TOI contact forces for warm starting
			// because they can be quite large.

			float h = subStep.Dt;

			// Integrate positions.
			for (int i = 0; i < BodyCount; ++i)
			{
				Vector2 c = _positions[i].C;
				float a = _positions[i].A;
				Vector2 v = _velocities[i].V;
				float w = _velocities[i].W;

				// Check for large velocities
				Vector2 translation = h * v;
				if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
				{
					float ratio = Settings.MaxTranslation / translation.Length();
					v *= ratio;
				}

				float rotation = h * w;
				if (rotation * rotation > Settings.MaxRotationSquared)
				{
					float ratio = Settings.MaxRotation / Math.Abs(rotation);
					w *= ratio;
				}

				// Integrate
				c += h * v;
				a += h * w;

				_positions[i].C = c;
				_positions[i].A = a;
				_velocities[i].V = v;
				_velocities[i].W = w;

				// Sync bodies
				Body body = Bodies[i];
				body._sweep.C = c;
				body._sweep.A = a;
				body._linearVelocity = v;
				body._angularVelocity = w;
				body.SynchronizeTransform();
			}

			Report(_contactSolver._velocityConstraints);
		}

		public void Add(Body body)
		{
			Debug.Assert(BodyCount < BodyCapacity);
			body.IslandIndex = BodyCount;
			Bodies[BodyCount++] = body;
		}

		public void Add(Contact contact)
		{
			Debug.Assert(ContactCount < ContactCapacity);
			_contacts[ContactCount++] = contact;
		}

		public void Add(Joint joint)
		{
			Debug.Assert(JointCount < JointCapacity);
			_joints[JointCount++] = joint;
		}

		void Report(ContactVelocityConstraint[] constraints)
		{
			if (_contactManager == null)
				return;

			for (int i = 0; i < ContactCount; ++i)
			{
				Contact c = _contacts[i];

				//FPE optimization: We don't store the impulses and send it to the delegate. We just send the whole contact.
				//FPE feature: added after collision
				if (c.FixtureA.AfterCollision != null)
					c.FixtureA.AfterCollision(c.FixtureA, c.FixtureB, c, constraints[i]);

				if (c.FixtureB.AfterCollision != null)
					c.FixtureB.AfterCollision(c.FixtureB, c.FixtureA, c, constraints[i]);

				if (_contactManager.OnPostSolve != null)
				{
					_contactManager.OnPostSolve(c, constraints[i]);
				}
			}
		}
	}
}