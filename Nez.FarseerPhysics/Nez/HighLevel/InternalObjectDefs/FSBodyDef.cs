using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	internal class FSBodyDef
	{
		public BodyType BodyType = BodyType.Static;
		public Vector2 LinearVelocity;
		public float AngularVelocity;
		public float LinearDamping;
		public float AngularDamping;

		public bool IsBullet;
		public bool IsSleepingAllowed = true;
		public bool IsAwake = true;
		public bool FixedRotation;
		public bool IgnoreGravity;
		public float GravityScale = 1;
		public float Mass;
		public float Inertia;
	}
}