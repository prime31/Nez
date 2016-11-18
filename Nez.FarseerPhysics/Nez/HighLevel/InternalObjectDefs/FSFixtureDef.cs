using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	internal class FSFixtureDef
	{
		public Shape shape;
		public float friction = 0.2f;
		public float restitution;
		public float density = 1f;
		public bool isSensor;
		public Category collidesWith = Settings.defaultFixtureCollidesWith;
		public Category collisionCategories = Settings.defaultFixtureCollisionCategories;
		public Category ignoreCCDWith = Settings.defaultFixtureIgnoreCCDWith;
		public short collisionGroup;
	}
}