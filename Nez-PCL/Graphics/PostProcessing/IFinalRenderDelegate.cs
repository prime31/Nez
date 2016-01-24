using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// optional interface that can be added to any object (or a PostProcessor)
	/// </summary>
	public interface IFinalRenderDelegate
	{
		Scene scene { get; set; }

		void onAddedToScene();

		void onSceneBackBufferSizeChanged( int newWidth, int newHeight );

		void handleFinalRender( Color letterboxColor, RenderTexture source, Rectangle finalRenderDestinationRect, SamplerState samplerState );

		void unload();
	}
}

