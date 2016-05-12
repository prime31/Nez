using System;
using Nez;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// used by the WaterReflectionPlane
	/// </summary>
	public class WaterReflectionMaterial : Material<WaterReflectionEffect>
	{
		RenderTarget2D _renderTarget;


		public WaterReflectionMaterial() : base( new WaterReflectionEffect() )
		{}


		public override void onPreRender( Camera camera )
		{
			var boundRenderTarget = Core.graphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;

			// only update the Shader when the renderTarget changes. it will be swapped out whenever the GraphicsDevice resets.
			if( _renderTarget == null || _renderTarget != boundRenderTarget )
			{
				_renderTarget = boundRenderTarget;
				typedEffect.renderTexture = boundRenderTarget;
			}

			typedEffect.matrixTransform = camera.viewProjectionMatrix;
		}
	}
}

