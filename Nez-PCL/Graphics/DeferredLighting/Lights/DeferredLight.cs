using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	public abstract class DeferredLight : RenderableComponent
	{
		/// <summary>
		/// we dont render lights normally so this method will do nothing
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		public override void render( Graphics graphics, Camera camera )
		{}
	}
}

