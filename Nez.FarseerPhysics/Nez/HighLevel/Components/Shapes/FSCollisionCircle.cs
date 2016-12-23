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
			_fixtureDef.shape = new CircleShape();
		}


		public FSCollisionCircle( float radius ) : this()
		{
			_radius = radius;
			_fixtureDef.shape.radius = _radius * FSConvert.displayToSim;
		}


		#region Configuration

		public FSCollisionCircle setRadius( float radius )
		{
			_radius = radius;
			recreateFixture();
			return this;
		}


		public FSCollisionCircle setCenter( Vector2 center )
		{
			_center = center;
			recreateFixture();
			return this;
		}

		#endregion


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			if( comp == Transform.Component.Scale )
				recreateFixture();
		}


		void recreateFixture()
		{
			_fixtureDef.shape.radius = _radius * transform.scale.X * FSConvert.displayToSim;
			( _fixtureDef.shape as CircleShape ).position = FSConvert.displayToSim * _center;

			if( _fixture != null )
			{
				var circleShape = _fixture.shape as CircleShape;
				circleShape.radius = _fixtureDef.shape.radius;
				circleShape.position = FSConvert.displayToSim * _center;

				// wake the body if it is asleep to update collisions
				wakeAnyContactingBodies();
			}
		}

	}
}
