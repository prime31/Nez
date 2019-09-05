using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionCircle : FSCollisionShape
	{
		Vector2 _center;
		float _radius = 0.1f;


		public FSCollisionCircle()
		{
			_fixtureDef.Shape = new CircleShape();
		}


		public FSCollisionCircle(float radius) : this()
		{
			_radius = radius;
			_fixtureDef.Shape.Radius = _radius * FSConvert.DisplayToSim;
		}


		#region Configuration

		public FSCollisionCircle SetRadius(float radius)
		{
			_radius = radius;
			RecreateFixture();
			return this;
		}


		public FSCollisionCircle SetCenter(Vector2 center)
		{
			_center = center;
			RecreateFixture();
			return this;
		}

		#endregion


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			if (comp == Transform.Component.Scale)
				RecreateFixture();
		}


		void RecreateFixture()
		{
			_fixtureDef.Shape.Radius = _radius * Transform.Scale.X * FSConvert.DisplayToSim;
			(_fixtureDef.Shape as CircleShape).Position = FSConvert.DisplayToSim * _center;

			if (_fixture != null)
			{
				var circleShape = _fixture.Shape as CircleShape;
				circleShape.Radius = _fixtureDef.Shape.Radius;
				circleShape.Position = FSConvert.DisplayToSim * _center;

				// wake the body if it is asleep to update collisions
				WakeAnyContactingBodies();
			}
		}
	}
}