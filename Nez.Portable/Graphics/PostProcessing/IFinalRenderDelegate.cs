using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// optional interface that can be added to any object for special cases where the final render to screen needs to be overridden. Note that
	/// the Scene.screenshotRequestCallback will not function if an IFinalRenderDelegate is present.
	/// </summary>
	public interface IFinalRenderDelegate
	{
		Scene scene { get; set; }

		void onAddedToScene();

		void onSceneBackBufferSizeChanged( int newWidth, int newHeight );

		void handleFinalRender( Color letterboxColor, RenderTarget2D source, Rectangle finalRenderDestinationRect, SamplerState samplerState );

		void unload();
	}
}

