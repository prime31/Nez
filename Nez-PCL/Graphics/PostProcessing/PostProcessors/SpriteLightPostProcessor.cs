using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// post processor to assist with making blended sprite lights. Usage is as follows:
	/// - render all sprite lights with a separate Renderer to a RenderTarget. The clear color or the Renderer is your ambient light color.
	/// - render all normal objects in standard fashion
	/// - add this PostProcessor with the RenderTarget from your lights Renderer
	/// </summary>
	public class SpriteLightPostProcessor : PostProcessor
	{
		/// <summary>
		/// multiplicative factor for the blend of the base and light render targets. Defaults to 1.
		/// </summary>
		/// <value>The multiplicative factor.</value>
		public float multiplicativeFactor
		{
			set
			{
				if( effect != null )
					effect.Parameters["_multiplicativeFactor"].SetValue( value );
				else
					_multiplicativeFactor = value;
			}
		}
		float _multiplicativeFactor = 1f;

		RenderTexture _lightsRenderTexture;


		public SpriteLightPostProcessor( int executionOrder, RenderTexture lightsRenderTexture ) : base( executionOrder )
		{
			_lightsRenderTexture = lightsRenderTexture;
		}


		public override void onAddedToScene()
		{
			effect = scene.contentManager.loadEffect<Effect>( "spriteLightMultiply", EffectResource.spriteLightMultiplyBytes );
			effect.Parameters["lightTexture"].SetValue( _lightsRenderTexture );
			effect.Parameters["_multiplicativeFactor"].SetValue( _multiplicativeFactor );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			Core.graphicsDevice.setRenderTarget( destination );
			Graphics.instance.batcher.begin( effect: effect );
			Graphics.instance.batcher.draw( source, new Rectangle( 0, 0, destination.Width, destination.Height ), Color.White );
			Graphics.instance.batcher.end();
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			// when the RenderTexture changes we have to reset the shader param since the underlying RenderTarget will be different
			effect.Parameters["lightTexture"].SetValue( _lightsRenderTexture );
		}

	}
}

