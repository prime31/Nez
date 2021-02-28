using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Nez.Analysis;


namespace Nez.Farseer
{
	public class FSWorld : SceneComponent
	{
		public World World;

		/// <summary>
		/// minimum delta time step for the simulation. The min of Time.deltaTime and this will be used for the physics step
		/// </summary>
		public float MinimumUpdateDeltaTime = 1f / 30;

		/// <summary>
		/// if true, the left mouse button will be used for picking and dragging physics objects around
		/// </summary>
		public bool EnableMousePicking;

		FixedMouseJoint _mouseJoint;


		public FSWorld() : this(new Vector2(0, 9.82f))
		{
		}


		public FSWorld(Vector2 gravity)
		{
			World = new World(gravity);
		}


		public FSWorld SetEnableMousePicking(bool enableMousePicking)
		{
			this.EnableMousePicking = enableMousePicking;
			return this;
		}


		#region SceneComponent

		public override void OnEnabled()
		{
			World.Enabled = true;
		}


		public override void OnDisabled()
		{
			World.Enabled = false;
		}


		public override void OnRemovedFromScene()
		{
			World.Clear();
			World = null;
		}


		public override void Update()
		{
			if (EnableMousePicking)
			{
				if (Input.LeftMouseButtonPressed)
				{
					var pos = Core.Scene.Camera.ScreenToWorldPoint(Input.MousePosition);
					var fixture = World.TestPoint(FSConvert.DisplayToSim * pos);
					if (fixture != null && !fixture.Body.IsStatic && !fixture.Body.IsKinematic)
						_mouseJoint = fixture.Body.CreateFixedMouseJoint(pos);
				}

				if (Input.LeftMouseButtonDown && _mouseJoint != null)
				{
					var pos = Core.Scene.Camera.ScreenToWorldPoint(Input.MousePosition);
					_mouseJoint.WorldAnchorB = FSConvert.DisplayToSim * pos;
				}

				if (Input.LeftMouseButtonReleased && _mouseJoint != null)
				{
					World.RemoveJoint(_mouseJoint);
					_mouseJoint = null;
				}
			}

			World.Step(MathHelper.Min(Time.DeltaTime, MinimumUpdateDeltaTime));
		}

		#endregion


		#region World Querying

		#endregion


		public static implicit operator World(FSWorld self)
		{
			return self.World;
		}
	}
}