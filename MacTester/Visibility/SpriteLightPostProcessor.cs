using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// post processor to assist with making blended sprite lights. Usage is as follows:
	/// - render all sprite lights with a separate Renderer to a RenderTarget. The clear color is your ambient light color.
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

		RenderTarget2D _lightsRenderTarget;


		public SpriteLightPostProcessor( int executionOrder, RenderTarget2D lightsRenderTarget ) : base( executionOrder )
		{
			_lightsRenderTarget = lightsRenderTarget;
		}


		public override void onAddedToScene()
		{
			effect = scene.contentManager.loadEffect( "Content/Effects/SpriteLightMultiply.mgfxo" );
			effect.Parameters["lightTexture"].SetValue( _lightsRenderTarget );
			effect.Parameters["_multiplicativeFactor"].SetValue( _multiplicativeFactor );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			Core.graphicsDevice.SetRenderTarget( destination );

			Graphics.instance.spriteBatch.Begin( effect: effect );
			Graphics.instance.spriteBatch.Draw( source, Vector2.Zero );
			Graphics.instance.spriteBatch.End();
		}

	}
}

