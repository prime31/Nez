using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Nez.Analysis;

namespace Nez.Farseer
{
	public class FSWorld : SceneComponent
	{
		public World world;

		/// <summary>
		/// minimum delta time step for the simulation. The min of Time.deltaTime and this will be used for the physics step
		/// </summary>
		public float minimumUpdateDeltaTime = 1f / 30;

		/// <summary>
		/// if true, the left mouse button will be used for picking and dragging physics objects around
		/// </summary>
		public bool enableMousePicking;

		FixedMouseJoint _mouseJoint;


		public FSWorld() : this( new Vector2( 0, 9.82f ) )
		{}


		public FSWorld( Vector2 gravity )
		{
			world = new World( gravity );
		}


		public FSWorld setEnableMousePicking( bool enableMousePicking )
		{
			this.enableMousePicking = enableMousePicking;
			return this;
		}


		#region SceneComponent

		public override void onEnabled()
		{
			world.enabled = true;
		}


		public override void onDisabled()
		{
			world.enabled = false;
		}


		public override void onRemovedFromScene()
		{
			world.clear();
			world = null;
		}


		public override void update()
		{
			if( enableMousePicking )
			{
				if( Input.leftMouseButtonPressed )
				{
					var pos = Core.scene.camera.screenToWorldPoint( Input.mousePosition );
					var fixture = world.testPoint( FSConvert.displayToSim * pos );
					if( fixture != null && !fixture.body.isStatic && !fixture.body.isKinematic )
						_mouseJoint = fixture.body.createFixedMouseJoint( pos );
				}

				if( Input.leftMouseButtonDown && _mouseJoint != null )
				{
					var pos = Core.scene.camera.screenToWorldPoint( Input.mousePosition );
					_mouseJoint.worldAnchorB = FSConvert.displayToSim * pos;
				}

				if( Input.leftMouseButtonReleased && _mouseJoint != null )
				{
					world.removeJoint( _mouseJoint );
					_mouseJoint = null;
				}
			}

			#if DEBUG
			TimeRuler.instance.beginMark( "physics", Color.Blue );
			#endif
			world.step( MathHelper.Min( Time.deltaTime, minimumUpdateDeltaTime ) );
			#if DEBUG
			TimeRuler.instance.endMark( "physics" );
			#endif
		}

		#endregion


		#region World Querying



		#endregion


		public static implicit operator World( FSWorld self )
		{
			return self.world;
		}

	}
}
