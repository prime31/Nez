using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Factories
{
	public static class LinkFactory
	{
		/// <summary>
		/// Creates the chain.
		/// </summary>
		/// <returns>The chain.</returns>
		/// <param name="world">World.</param>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="linkWidth">Link width.</param>
		/// <param name="linkHeight">Link height.</param>
		/// <param name="numberOfLinks">Number of links.</param>
		/// <param name="linkDensity">Link density.</param>
		/// <param name="attachRopeJoint">Creates a rope joint between start and end. This enforces the length of the rope. Said in another way: it makes the rope less bouncy.</param>
		/// <param name="fixStart">If set to <c>true</c> fix start.</param>
		/// <param name="fixEnd">If set to <c>true</c> fix end.</param>
		public static List<Body> CreateChain(World world, Vector2 start, Vector2 end, float linkWidth, float linkHeight,
		                                     int numberOfLinks, float linkDensity, bool attachRopeJoint,
		                                     bool fixStart = false, bool fixEnd = false)
		{
			Debug.Assert(numberOfLinks >= 2);

			// Chain start / end
			var path = new Path();
			path.Add(start);
			path.Add(end);

			// A single chainlink
			var shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);

			// Use PathManager to create all the chainlinks based on the chainlink created before.
			var chainLinks =
				PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);

			if (fixStart)
			{
				// Fix the first chainlink to the world
				var axle = BodyFactory.CreateCircle(world, 0.1f, 1, chainLinks[0].Position);
				JointFactory.CreateRevoluteJoint(world, chainLinks[0], axle, new Vector2(0, -(linkHeight / 2)),
					Vector2.Zero);
			}

			if (fixEnd)
			{
				// Fix the last chainlink to the world
				var lastIndex = chainLinks.Count - 1;
				var axle = BodyFactory.CreateCircle(world, 0.1f, 1, chainLinks[lastIndex].Position);
				JointFactory.CreateRevoluteJoint(world, chainLinks[lastIndex], axle, new Vector2(0, -(linkHeight / 2)),
					Vector2.Zero);
			}

			// Attach all the chainlinks together with a revolute joint
			PathManager.AttachBodiesWithRevoluteJoint(world, chainLinks, new Vector2(0, -linkHeight),
				new Vector2(0, linkHeight), false, false);

			if (attachRopeJoint)
				JointFactory.CreateRopeJoint(world, chainLinks[0], chainLinks[chainLinks.Count - 1], Vector2.Zero,
					Vector2.Zero);

			return chainLinks;
		}
	}
}