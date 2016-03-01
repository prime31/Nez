using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace Nez.Textures
{
	public class RenderTarget
	{
		/// <summary>
		/// internal class with additional lastFrameUsed field for managing temporary RenderTargets
		/// </summary>
		internal class TrackedRenderTarget2D : RenderTarget2D
		{
			public uint lastFrameUsed;


			public TrackedRenderTarget2D( int width, int height, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat ) : base( Core.graphicsDevice, width, height, false, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.PreserveContents )
			{}
		}


		internal static RenderTarget instance;
		List<TrackedRenderTarget2D> _renderTargetPool = new List<TrackedRenderTarget2D>();


		#region Temporary RenderTarget management

		/// <summary>
		/// gets a temporary RenderTarget2D from the pool. When done using it call releaseTemporary to put it back in the pool. Note that the
		/// contents of the RenderTarget2D could be anything at all so clear it if you need to before using it.
		/// </summary>
		/// <returns>The temporary.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static RenderTarget2D getTemporary( int width, int height )
		{
			return getTemporary( width, height, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// gets a temporary RenderTarget2D from the pool. When done using it call releaseTemporary to put it back in the pool. Note that the
		/// contents of the RenderTarget2D could be anything at all so clear it if you need to before using it.
		/// </summary>
		/// <returns>The temporary.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="depthFormat">Depth format.</param>
		public static RenderTarget2D getTemporary( int width, int height, DepthFormat depthFormat )
		{
			RenderTarget2D tempRenderTarget = null;
			int tempRenderTargetIndex = -1;
			for( var i = 0; i < instance._renderTargetPool.Count; i++ )
			{
				var renderTarget = instance._renderTargetPool[i];
				if( renderTarget.Width == width && renderTarget.Height == height && renderTarget.DepthStencilFormat == depthFormat )
				{
					tempRenderTarget = renderTarget;
					tempRenderTargetIndex = i;
					break;
				}
			}

			if( tempRenderTargetIndex >= 0 )
			{
				instance._renderTargetPool.RemoveAt( tempRenderTargetIndex );
				return tempRenderTarget;
			}

			// if we get here, we need to create a fresh RenderTarget2D
			return new TrackedRenderTarget2D( width, height, SurfaceFormat.Color, depthFormat );
		}


		/// <summary>
		/// puts a temporary RenderTarget2D back in the pool. Do not attempt to put RenderTarget2Ds in the pool that were not acquired via getTemporary.
		/// </summary>
		/// <param name="renderTarget">Render target.</param>
		public static void releaseTemporary( RenderTarget2D renderTarget )
		{
			Assert.isTrue( renderTarget is TrackedRenderTarget2D, "Attempted to release a temporary RenderTarget2D that is not managed by the system" );

			var trackedRT = renderTarget as TrackedRenderTarget2D;
			trackedRT.lastFrameUsed = Time.frameCount;
			instance._renderTargetPool.Add( trackedRT );
		}

		#endregion


		#region RenderTarget2D creation helpers

		/// <summary>
		/// helper for creating a full screen RenderTarget2D
		/// </summary>
		public static RenderTarget2D create()
		{
			return create( Screen.backBufferWidth, Screen.backBufferHeight, Screen.backBufferFormat, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// helper for creating a full screen RenderTarget2D with a specific DepthFormat
		/// </summary>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public static RenderTarget2D create( DepthFormat preferredDepthFormat )
		{
			return create( Screen.backBufferWidth, Screen.backBufferHeight, Screen.backBufferFormat, preferredDepthFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static RenderTarget2D create( int width, int height )
		{
			return create( width, height, Screen.backBufferFormat, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public static RenderTarget2D create( int width, int height, DepthFormat preferredDepthFormat )
		{
			return create( width, height, Screen.backBufferFormat, preferredDepthFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="preferredFormat">Preferred format.</param>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public static RenderTarget2D create( int width, int height, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat )
		{
			return new RenderTarget2D( Core.graphicsDevice, width, height, false, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.PreserveContents );
		}

		#endregion


		internal void update()
		{
			// remove any TrackedRanderTarget2Ds that havent been used for 2 frames or more
			for( var i = _renderTargetPool.Count - 1; i >= 0; i-- )
			{
				if( _renderTargetPool[i].lastFrameUsed + 2 < Time.frameCount )
				{
					_renderTargetPool[i].Dispose();
					_renderTargetPool.RemoveAt( i );
				}
			}
		}
	}
}

