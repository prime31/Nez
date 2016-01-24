using System;
using Nez.Systems;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;


namespace Nez
{
	/// <summary>
	/// this PostProcessor expects that the layerRenderTexture is the top-most layer and that it contains 
	/// </summary>
	public class PixelBloomPostProcessor : BloomPostProcessor
	{
		RenderTexture _layerRT;
		RenderTexture _tempRT;


		public PixelBloomPostProcessor( RenderTexture layerRenderTexture, int executionOrder ) : base( executionOrder )
		{
			_layerRT = layerRenderTexture;
			_tempRT = new RenderTexture( layerRenderTexture.renderTarget2D.Width, layerRenderTexture.renderTarget2D.Height );
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			base.onSceneBackBufferSizeChanged( newWidth, newHeight );

			_tempRT.resize( newHeight, newWidth );
		}


		public override void process( RenderTexture source, RenderTexture destination )
		{
			// first we process the rendered layer with the bloom effect
			base.process( _layerRT, _tempRT );

			// we need to be careful here and ensure we use AlphaBlending since the layer we rendered is mostly transparent
			Core.graphicsDevice.SetRenderTarget( destination );
			Graphics.instance.spriteBatch.Begin( 0, BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect );

			// now we first draw the full scene (source), then draw our bloomed layer (tempRT) then draw the un-bloomed layer (layerRT)
			Graphics.instance.spriteBatch.Draw( source, new Rectangle( 0, 0, destination.renderTarget2D.Width, destination.renderTarget2D.Height ), Color.White );
			Graphics.instance.spriteBatch.Draw( _tempRT, new Rectangle( 0, 0, destination.renderTarget2D.Width, destination.renderTarget2D.Height ), Color.White );
			Graphics.instance.spriteBatch.Draw( _layerRT, new Rectangle( 0, 0, destination.renderTarget2D.Width, destination.renderTarget2D.Height ), Color.White );

			Graphics.instance.spriteBatch.End();
		}
	}
}

