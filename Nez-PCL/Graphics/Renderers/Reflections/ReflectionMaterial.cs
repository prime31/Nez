using System;
using Nez;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// used in conjunction with the ReflectionRenderer
	/// </summary>
	public class ReflectionMaterial : Material<ReflectionEffect>
	{
		public RenderTexture renderTexture;

		RenderTarget2D _renderTarget;


		public ReflectionMaterial( ReflectionRenderer reflectionRenderer ) : base( new ReflectionEffect() )
		{
			renderTexture = reflectionRenderer.renderTexture;
		}


		public override void onPreRender( Camera camera )
		{
			// only update the Shader when the renderTarget changes. it will be swapped out whenever the GraphicsDevice resets.
			if( _renderTarget == null || _renderTarget != renderTexture.renderTarget )
			{
				_renderTarget = renderTexture.renderTarget;
				typedEffect.renderTexture = renderTexture.renderTarget;
			}

			typedEffect.matrixTransform = camera.viewProjectionMatrix;
		}
	}
}

