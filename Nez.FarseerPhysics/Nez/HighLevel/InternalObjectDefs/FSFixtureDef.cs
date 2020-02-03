using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	internal class FSFixtureDef
	{
		public Shape Shape;
		public float Friction = 0.2f;
		public float Restitution;
		public float Density = 1f;
		public bool IsSensor;
		public Category CollidesWith = Settings.DefaultFixtureCollidesWith;
		public Category CollisionCategories = Settings.DefaultFixtureCollisionCategories;
		public Category IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
		public short CollisionGroup;
	}
}