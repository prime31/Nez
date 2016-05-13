using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public static class GraphicsDeviceExt
	{
		static RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[1];


		/// <summary>
		/// sets the RenderTarget without allocating a RenderTargetBinding array.
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="renderTarget">Render target.</param>
		public static void setRenderTarget( this GraphicsDevice self, RenderTarget2D renderTarget )
		{
			if( renderTarget == null )
			{
				self.SetRenderTargets( null );
			}
			else
			{
				_renderTargetBinding[0] = renderTarget;
				self.SetRenderTargets( _renderTargetBinding );
			}
		}
	}
}

