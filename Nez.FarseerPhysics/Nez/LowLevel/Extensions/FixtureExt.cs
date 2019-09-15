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
		public static bool CollidesWithAnyFixtures(this Fixture self, ref Vector2 motion, out FSCollisionResult result)
		{
			result = new FSCollisionResult();
			motion *= FSConvert.DisplayToSim;
			AABB aabb;
			FSTransform xf;
			var didCollide = false;

			self.Body.GetTransform(out xf);
			xf.P += motion;
			self.Shape.ComputeAABB(out aabb, ref xf, 0);

			var neighbors = ListPool<Fixture>.Obtain();
			self.Body.World.QueryAABB(ref aabb, neighbors);
			if (neighbors.Count > 1)
			{
				// handle collisions with all but ourself
				for (var i = 0; i < neighbors.Count; i++)
				{
					if (neighbors[i].FixtureId == self.FixtureId)
						continue;

					if (FSCollisions.CollideFixtures(self, ref motion, neighbors[i], out result))
					{
						// if we have a collision, adjust the transform to account for it
						xf.P += result.MinimumTranslationVector;
					}
				}
			}

			ListPool<Fixture>.Free(neighbors);
			motion *= FSConvert.SimToDisplay;

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
		public static bool CollideFixtures(this Fixture self, Fixture fixtureB, out FSCollisionResult result)
		{
			return FSCollisions.CollideFixtures(self, fixtureB, out result);
		}
	}
}