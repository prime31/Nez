using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FSTransform = FarseerPhysics.Common.Transform;


namespace Nez.Farseer
{
	public static class FixtureExt
	{
		/// <summary>
		/// checks to see if the Fixture with motion applied (delta movement vector) collides with any collider. If it does, true will be
		/// returned and result will be populated with collision data. motion will be set to the maximum distance the Body can travel
		/// before colliding.
		/// </summary>
		/// <returns><c>true</c>, if with any was collidesed, <c>false</c> otherwise.</returns>
		/// <param name="self">Fixture a.</param>
		/// <param name="motion">the delta movement in Nez pixel coordinates</param>
		/// <param name="result">Result.</param>
		public static bool collidesWithAnyFixtures( this Fixture self, ref Vector2 motion, out FSCollisionResult result )
		{
			result = new FSCollisionResult();
			motion *= FSConvert.displayToSim;
			AABB aabb;
			FSTransform xf;
			var didCollide = false;

			self.body.getTransform( out xf );
			xf.p += motion;
			self.shape.computeAABB( out aabb, ref xf, 0 );

			var neighbors = ListPool<Fixture>.obtain();
			self.body.world.queryAABB( ref aabb, neighbors );
			if( neighbors.Count > 1 )
			{
				// handle collisions with all but ourself
				for( var i = 0; i < neighbors.Count; i++ )
				{
					if( neighbors[i].fixtureId == self.fixtureId )
						continue;

					if( FSCollisions.collideFixtures( self, ref motion, neighbors[i], out result ) )
					{
						// if we have a collision, adjust the transform to account for it
						xf.p += result.minimumTranslationVector;
					}
				}
			}

			ListPool<Fixture>.free( neighbors );
			motion *= FSConvert.simToDisplay;

			return didCollide;
		}


		/// <summary>
		/// checks for collisions between two Fixtures. Note that the first Fixture must have a Circle/PolygonShape and one of the Fixtures must be
		/// static for a collision to occur.
		/// </summary>
		/// <returns><c>true</c>, if fixtures was collided, <c>false</c> otherwise.</returns>
		/// <param name="self">Self.</param>
		/// <param name="fixtureB">Fixture b.</param>
		/// <param name="result">Result.</param>
		public static bool collideFixtures( this Fixture self, Fixture fixtureB, out FSCollisionResult result )
		{
			return FSCollisions.collideFixtures( self, fixtureB, out result );
		}

	}
}
