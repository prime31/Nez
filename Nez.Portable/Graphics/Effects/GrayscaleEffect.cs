using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class GrayscaleEffect : Effect
	{
		public GrayscaleEffect() : base( Core.graphicsDevice, EffectResource.grayscaleBytes )
		{
		}
	}
}

