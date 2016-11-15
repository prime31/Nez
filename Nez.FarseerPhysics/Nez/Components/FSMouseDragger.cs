using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;


namespace Nez.Farseer
{
	public class FSMouseDragger : Component, IUpdatable
	{
		World _world;
		FixedMouseJoint _mouseJoint;


		public override void onAddedToEntity()
		{
			_world = ( entity.scene as FarseerScene ).world;
		}


		void IUpdatable.update()
		{
			if( Input.leftMouseButtonPressed )
			{
				var pos = entity.scene.camera.screenToWorldPoint( Input.mousePosition );
				var fixture = _world.TestPoint( FSConvert.displayToSim * pos );
				if( fixture != null && !fixture.body.isStatic && !fixture.body.isKinematic )
					_mouseJoint = fixture.body.createFixedMouseJoint( pos );
			}

			if( Input.leftMouseButtonDown && _mouseJoint != null )
			{
				var pos = entity.scene.camera.screenToWorldPoint( Input.mousePosition );
				_mouseJoint.worldAnchorB = FSConvert.toSimUnits( pos );
			}

			if( Input.leftMouseButtonReleased && _mouseJoint != null )
			{
				_world.RemoveJoint( _mouseJoint );
				_mouseJoint = null;
			}
		}
	}
}
