using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSFrictionJoint : FSJoint
	{
		FSFrictionJointDef _jointDef = new FSFrictionJointDef();
		Vector2 _anchor;


		#region Configuration

		public FSFrictionJoint SetAnchor(Vector2 anchor)
		{
			_anchor = anchor;
			RecreateJoint();
			return this;
		}


		public FSFrictionJoint SetMaxForce(float maxForce)
		{
			_jointDef.MaxForce = maxForce;
			RecreateJoint();
			return this;
		}


		public FSFrictionJoint SetMaxTorque(float maxTorque)
		{
			_jointDef.MaxTorque = maxTorque;
			RecreateJoint();
			return this;
		}

		#endregion


		internal override FSJointDef GetJointDef()
		{
			InitializeJointDef(_jointDef);
			if (_jointDef.BodyA == null || _jointDef.BodyB == null)
				return null;

			_jointDef.Anchor = FSConvert.DisplayToSim * _anchor;

			return _jointDef;
		}
	}
}