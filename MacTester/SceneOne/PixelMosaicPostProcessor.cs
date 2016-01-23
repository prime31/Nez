using System;
using Nez;
using Nez.Systems;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class PixelMosaicPostProcessor : PostProcessor
	{
		Texture2D _mosaicTexture;
		RenderTexture _renderTex;


		public PixelMosaicPostProcessor( NezContentManager contentManager ) : base( 5 )
		{
			effect = contentManager.LoadEffect( "Effects/MultiTextureOverlay.mgfxo" );
				
			_mosaicTexture = new Texture2D( Core.graphicsDevice, 3, 3 );
			var colors = new uint[3 * 3];
			colors[2] = 0xff808080;
			colors[6] = 0xff808080;
			colors[0] = 0xffffffff;
			colors[8] = 0xff000000;

			colors[3] = 0xffE0E0E0;
			colors[4] = 0xffffffff;
			colors[5] = 0xff000000;
			colors[1] = 0xffffffff;
			colors[7] = 0xff000000;
			_mosaicTexture.SetData<uint>( colors );

			effect.Parameters["secondTexture"].SetValue( _mosaicTexture );
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			if( _renderTex != null )
				_renderTex.resize( newWidth, newHeight );
			else
				_renderTex = new RenderTexture( newWidth, newHeight );

			// based on the look of games by: http://deepnight.net/games/strike-of-rage/
			// use the mosaic to render to a full sized RenderTexture repeating the mosaic
			Core.graphicsDevice.SetRenderTarget( _renderTex );
			Graphics.instance.spriteBatch.Begin( 0, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect );
			Graphics.instance.spriteBatch.Draw( _mosaicTexture, Vector2.Zero, new Rectangle( 0, 0, _renderTex.renderTarget2D.Width, _renderTex.renderTarget2D.Height ), Color.White );
			Graphics.instance.spriteBatch.End();

			// let our Effect know
			effect.Parameters["secondTexture"].SetValue( _renderTex );
		}


		public override void process( RenderTexture source, RenderTexture destination )
		{
			drawFullscreenQuad( source, destination, effect );
		}


		public override void unload()
		{
			_mosaicTexture.Dispose();
			_renderTex.unload();
		}

	}
}

