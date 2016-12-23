using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public static partial class Farseer
	{
		public static class LinkFactory
		{
			/// <summary>
			/// Creates a chain.
			/// </summary>
			/// <param name="world">The world.</param>
			/// <param name="start">The start.</param>
			/// <param name="end">The end.</param>
			/// <param name="linkWidth">The width.</param>
			/// <param name="linkHeight">The height.</param>
			/// <param name="numberOfLinks">The number of links.</param>
			/// <param name="linkDensity">The link density.</param>
			/// <param name="attachRopeJoint">Creates a rope joint between start and end. This enforces the length of the rope. Said in another way: it makes the rope less bouncy.</param>
			/// <returns></returns>
			public static List<Body> createChain( World world, Vector2 start, Vector2 end, float linkWidth, float linkHeight, int numberOfLinks, float linkDensity, bool attachRopeJoint, bool fixStart = false, bool fixEnd = false )
			{
				return FarseerPhysics.Factories.LinkFactory.createChain( world, FSConvert.displayToSim * start, FSConvert.toSimUnits( end ), FSConvert.displayToSim * linkWidth, FSConvert.displayToSim * linkHeight, numberOfLinks, linkDensity, attachRopeJoint, fixStart, fixEnd );
			}
		}
	}
}
