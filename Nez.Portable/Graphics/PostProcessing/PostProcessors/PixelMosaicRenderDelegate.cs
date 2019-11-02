using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// overlays a mosaic on top of the final render. Useful only for pixel perfect pixel art.
	/// </summary>
	public class PixelMosaicRenderDelegate : IFinalRenderDelegate
	{
		Scene _scene;
		Effect _effect;
		Texture2D _mosaicTexture;
		RenderTarget2D _mosaicRenderTex;
		int _lastMosaicScale = -1;


		public void OnAddedToScene(Scene scene)
		{
			_scene = scene;
			_effect = scene.Content.LoadEffect<Effect>("multiTextureOverlay", EffectResource.MultiTextureOverlayBytes);
		}

		void CreateMosaicTexture(int size)
		{
			if (_mosaicTexture != null)
				_mosaicTexture.Dispose();

			_mosaicTexture = new Texture2D(Core.GraphicsDevice, size, size);
			var colors = new uint[size * size];

			for (var i = 0; i < colors.Length; i++)
				colors[i] = 0x808080;

			colors[0] = 0xffffffff;
			colors[size * size - 1] = 0xff000000;

			for (var x = 1; x < size - 1; x++)
			{
				colors[x * size] = 0xffE0E0E0;
				colors[x * size + 1] = 0xffffffff;
				colors[x * size + size - 1] = 0xff000000;
			}

			for (var y = 1; y < size - 1; y++)
			{
				colors[y] = 0xffffffff;
				colors[(size - 1) * size + y] = 0xff000000;
			}

			_mosaicTexture.SetData<uint>(colors);
			_effect.Parameters["_secondTexture"].SetValue(_mosaicTexture);
		}

		public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			// dont recreate the mosaic unless we really need to
			if (_lastMosaicScale != _scene.PixelPerfectScale)
			{
				CreateMosaicTexture(_scene.PixelPerfectScale);
				_lastMosaicScale = _scene.PixelPerfectScale;
			}

			if (_mosaicRenderTex != null)
			{
				_mosaicRenderTex.Dispose();
				_mosaicRenderTex = RenderTarget.Create(newWidth * _scene.PixelPerfectScale,
					newHeight * _scene.PixelPerfectScale, DepthFormat.None);
			}
			else
			{
				_mosaicRenderTex = RenderTarget.Create(newWidth * _scene.PixelPerfectScale,
					newHeight * _scene.PixelPerfectScale, DepthFormat.None);
			}

			// based on the look of games by: http://deepnight.net/games/strike-of-rage/
			// use the mosaic to render to a full sized RenderTarget repeating the mosaic
			Core.GraphicsDevice.SetRenderTarget(_mosaicRenderTex);
			Graphics.Instance.Batcher.Begin(BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None,
				RasterizerState.CullNone);
			Graphics.Instance.Batcher.Draw(_mosaicTexture, Vector2.Zero,
				new Rectangle(0, 0, _mosaicRenderTex.Width, _mosaicRenderTex.Height), Color.White);
			Graphics.Instance.Batcher.End();

			// let our Effect know about our rendered, full screen mosaic
			_effect.Parameters["_secondTexture"].SetValue(_mosaicRenderTex);
		}

		public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
		                              Rectangle finalRenderDestinationRect, SamplerState samplerState)
		{
			Core.GraphicsDevice.SetRenderTarget(finalRenderTarget);
			Core.GraphicsDevice.Clear(letterboxColor);
			Graphics.Instance.Batcher.Begin(BlendState.Opaque, samplerState, DepthStencilState.None,
				RasterizerState.CullNone, _effect);
			Graphics.Instance.Batcher.Draw(source, finalRenderDestinationRect, Color.White);
			Graphics.Instance.Batcher.End();
		}

		public void Unload()
		{
			_mosaicTexture.Dispose();
			_mosaicRenderTex.Dispose();
		}
	}
}