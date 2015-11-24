using System;


namespace Nez
{
	public enum CollisionDirection
	{
		Left,
		Right,
		Above,
		Below
	}

	public interface ICollisionCallback
	{
		void onCollisionEnter( Collider collider, CollisionDirection direction );
	}
}

