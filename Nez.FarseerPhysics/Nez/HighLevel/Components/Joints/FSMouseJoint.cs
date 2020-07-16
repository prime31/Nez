using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSMouseJoint : FSJoint, IUpdatable
	{
		FSMouseJointDef _jointDef = new FSMouseJointDef();


		#region Configuration

		public FSMouseJoint SetWorldAnchor(Vector2 worldAnchor)
		{
			_jointDef.WorldAnchor = worldAnchor;
			if (_joint != null)
				_joint.WorldAnchorB = worldAnchor * FSConvert.DisplayToSim;
			return this;
		}


		public FSMouseJoint SetFrequency(float frequency)
		{
			_jointDef.Frequency = frequency;
			if (_joint != null)
				(_joint as FixedMouseJoint).Frequency = frequency;
			return this;
		}


		public FSMouseJoint SetDampingRatio(float dampingRatio)
		{
			_jointDef.DampingRatio = dampingRatio;
			if (_joint != null)
				(_joint as FixedMouseJoint).DampingRatio = dampingRatio;
			return this;
		}


		public FSMouseJoint SetMaxForce(float maxForce)
		{
			_jointDef.MaxForce = maxForce;
			if (_joint != null)
				(_joint as FixedMouseJoint).MaxForce = maxForce;
			return this;
		}

		#endregion


		public virtual void Update()
		{
			if (_joint != null)
			{
				var pos = Core.Scene.Camera.ScreenToWorldPoint(Input.MousePosition);
				SetWorldAnchor(pos);
			}
		}


		internal override FSJointDef GetJointDef()
		{
			InitializeJointDef(_jointDef);
			if (_jointDef.BodyA == null)
				return null;

			return _jointDef;
		}
	}
}