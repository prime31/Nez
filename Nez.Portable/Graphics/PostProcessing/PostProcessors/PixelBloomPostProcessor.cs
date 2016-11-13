using System;
using Nez.Systems;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;


namespace Nez
{
	/// <summary>
	/// this PostProcessor expects that the layerRenderTarget is the top-most layer and that it contains 
	/// </summary>
	public class PixelBloomPostProcessor : BloomPostProcessor
	{
		RenderTexture _layerRT;
		RenderTexture _tempRT;


		public PixelBloomPostProcessor( RenderTexture layerRenderTexture, int executionOrder ) : base( executionOrder )
		{
			_layerRT = layerRenderTexture;
			_tempRT = new RenderTexture( _layerRT.renderTarget.Width, _layerRT.renderTarget.Height, DepthFormat.None );
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			base.onSceneBackBufferSizeChanged( newWidth, newHeight );

			_tempRT.resize( newWidth, newHeight );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			// first we process the rendered layer with the bloom effect
			base.process( _layerRT, _tempRT );

			// we need to be careful here and ensure we use AlphaBlending since the layer we rendered is mostly transparent
			Core.graphicsDevice.setRenderTarget( destination );
			Graphics.instance.batcher.begin( BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone );

			// now we first draw the full scene (source), then draw our bloomed layer (tempRT) then draw the un-bloomed layer (layerRT)
			Graphics.instance.batcher.draw( source, new Rectangle( 0, 0, destination.Width, destination.Height ), Color.White );
			Graphics.instance.batcher.draw( _tempRT, new Rectangle( 0, 0, destination.Width, destination.Height ), Color.White );
			Graphics.instance.batcher.draw( _layerRT, new Rectangle( 0, 0, destination.Width, destination.Height ), Color.White );

			Graphics.instance.batcher.end();
		}


		public override void unload()
		{
			base.unload();

			_tempRT.Dispose();
		}

	}
}

