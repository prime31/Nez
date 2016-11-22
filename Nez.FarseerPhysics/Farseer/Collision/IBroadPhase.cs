using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision
{
	public interface IBroadPhase
	{
		int proxyCount { get; }

		void updatePairs( BroadphaseDelegate callback );

		bool testOverlap( int proxyIdA, int proxyIdB );

		int addProxy( ref FixtureProxy proxy );

		void removeProxy( int proxyId );

		void moveProxy( int proxyId, ref AABB aabb, Vector2 displacement );

		FixtureProxy getProxy( int proxyId );

		void touchProxy( int proxyId );

		void getFatAABB( int proxyId, out AABB aabb );

		void query( Func<int, bool> callback, ref AABB aabb );

		void rayCast( Func<RayCastInput, int, float> callback, ref RayCastInput input );

		void shiftOrigin( Vector2 newOrigin );

	}
}