using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	internal class FSBodyDef
	{
		public BodyType bodyType = BodyType.Static;
		public Vector2 linearVelocity;
		public float angularVelocity;
		public float linearDamping;
		public float angularDamping;

		public bool isBullet;
		public bool isSleepingAllowed = true;
		public bool isAwake = true;
		public bool fixedRotation;
		public bool ignoreGravity;
		public float gravityScale = 1;
		public float mass;
		public float inertia;
	}
}
