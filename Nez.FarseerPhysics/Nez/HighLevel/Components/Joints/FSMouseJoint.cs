using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSMouseJoint : FSJoint, IUpdatable
	{
		FSMouseJointDef _jointDef = new FSMouseJointDef();


		#region Configuration

		public FSMouseJoint setWorldAnchor( Vector2 worldAnchor )
		{
			_jointDef.worldAnchor = worldAnchor;
			if( _joint != null )
				_joint.worldAnchorB = worldAnchor * FSConvert.displayToSim;
			return this;
		}


		public FSMouseJoint setFrequency( float frequency )
		{
			_jointDef.frequency = frequency;
			if( _joint != null )
				( _joint as FixedMouseJoint ).frequency = frequency;
			return this;
		}


		public FSMouseJoint setDampingRatio( float dampingRatio )
		{
			_jointDef.dampingRatio = dampingRatio;
			if( _joint != null )
				( _joint as FixedMouseJoint ).dampingRatio = dampingRatio;
			return this;
		}


		public FSMouseJoint setMaxForce( float maxForce )
		{
			_jointDef.maxForce = maxForce;
			if( _joint != null )
				( _joint as FixedMouseJoint ).maxForce = maxForce;
			return this;
		}

		#endregion


		void IUpdatable.update()
		{
			if( _joint != null )
			{
				var pos = Core.scene.camera.screenToWorldPoint( Input.mousePosition );
				setWorldAnchor( pos );
			}
		}


		internal override FSJointDef getJointDef()
		{
			initializeJointDef( _jointDef );
			if( _jointDef.bodyA == null )
				return null;

			return _jointDef;
		}

	}
}
