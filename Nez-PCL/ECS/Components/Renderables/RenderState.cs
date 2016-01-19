using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class RenderState
	{
		/// <summary>
		/// BlendState used by the SpriteBatch for the current RenderableComponent
		/// </summary>
		public BlendState blendState = BlendState.AlphaBlend;

		/// <summary>
		/// DepthStencilState used by the SpriteBatch for the current RenderableComponent
		/// </summary>
		public DepthStencilState depthStencilState = DepthStencilState.None;

		/// <summary>
		/// Effect used by the SpriteBatch for the current RenderableComponent
		/// </summary>
		public Effect effect;


		#region Static common states

		public static RenderState stencilWrite( int stencilRef = 1 )
		{
			return new RenderState {
				depthStencilState = new DepthStencilState {
					StencilEnable = true,
					StencilFunction = CompareFunction.Always,
					StencilPass = StencilOperation.Replace,
					ReferenceStencil = stencilRef,
					DepthBufferEnable = false,
				}
			};
		}


		public static RenderState stencilRead( int stencilRef = 1 )
		{
			return new RenderState( new DepthStencilState {
				StencilEnable = true,
				StencilFunction = CompareFunction.Equal,
				StencilPass = StencilOperation.Keep,
				ReferenceStencil = stencilRef,
				DepthBufferEnable = false
			});
		}


		public static RenderState blendDarken()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Min
				}
			};
		}


		public static RenderState blendLighten()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Max
				}
			};
		}


		public static RenderState blendMultiply()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.Zero,
					ColorBlendFunction = BlendFunction.Add,
				}
			};
		}


		public static RenderState blendLinearDodge()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static RenderState blendSubtractive()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.SourceAlpha,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.ReverseSubtract,
					AlphaSourceBlend = Blend.SourceAlpha,
					AlphaDestinationBlend = Blend.One,
					AlphaBlendFunction = BlendFunction.ReverseSubtract
				}
			};
		}

		#endregion


		public RenderState()
		{}


		public RenderState( Effect effect )
		{
			this.effect = effect;
		}


		public RenderState( BlendState blendState, Effect effect = null )
		{
			this.blendState = blendState;
			this.effect = effect;
		}


		public RenderState( DepthStencilState depthStencilState, Effect effect = null )
		{
			this.depthStencilState = depthStencilState;
			this.effect = effect;
		}


		/// <summary>
		/// called when the RenderState is initialy set right before SpriteBatch.Begin to allow any Effects to have parameters set if necessary
		/// based on the Camera Matrix. This will only be called if there is a non-null Effect.
		/// </summary>
		/// <param name="camera">Camera.</param>
		public virtual void onPreRender( Camera camera )
		{}

	}
}

