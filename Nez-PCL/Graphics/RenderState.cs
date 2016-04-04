using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class RenderState : IComparable<RenderState>
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
		/// SamplerState used by the SpriteBatch for the current RenderableComponent
		/// </summary>
		public SamplerState samplerState = Core.defaultSamplerState;

		/// <summary>
		/// Effect used by the SpriteBatch for the current RenderableComponent
		/// </summary>
		public Effect effect;


		#region Static common states

		// BlendStates can be made to work with transparency by adding the following:
		// - AlphaSourceBlend = Blend.SourceAlpha, 
		// - AlphaDestinationBlend = Blend.InverseSourceAlpha 

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
			return new RenderState
			{
				depthStencilState = new DepthStencilState {
					StencilEnable = true,
					StencilFunction = CompareFunction.Equal,
					StencilPass = StencilOperation.Keep,
					ReferenceStencil = stencilRef,
					DepthBufferEnable = false
				}
			};
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


		public static RenderState blendScreen()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static RenderState blendMultiply()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.Zero,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static RenderState blendMultiply2x()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.SourceColor,
					ColorBlendFunction = BlendFunction.Add
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


		public static RenderState blendLinearBurn()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.ReverseSubtract
				}
			};
		}


		public static RenderState blendDifference()
		{
			return new RenderState {
				blendState = new BlendState {
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.InverseSourceColor,
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
		/// called when the RenderState is initialy set right before SpriteBatch.Begin to allow any Effects that have parameters set if necessary
		/// based on the Camera Matrix. This will only be called if there is a non-null Effect.
		/// </summary>
		/// <param name="camera">Camera.</param>
		public virtual void onPreRender( Camera camera )
		{
			if( effect is AlphaTestEffect )
			{
				var alphaEffect = effect as AlphaTestEffect;
				alphaEffect.Projection = camera.getViewProjectionMatrix();
			}
		}


		/// <summary>
		/// sets the RenderState.effect as an AlphaTestEffect with the specified referenceAlpha. The projection matrix will be set each frame
		/// automatically for you.
		/// </summary>
		/// <param name="referenceAlpha">Reference alpha.</param>
		public void addAlphaTestEffect( int referenceAlpha = 127 )
		{
			var alphaEffect = new AlphaTestEffect( Core.graphicsDevice );
			alphaEffect.AlphaFunction = CompareFunction.Greater;
			alphaEffect.ReferenceAlpha = referenceAlpha;

			effect = alphaEffect;
		}


		/// <summary>
		/// disposes of all graphic-related assets
		/// </summary>
		public void unload()
		{
			if( blendState != BlendState.AlphaBlend )
				blendState.Dispose();

			if( depthStencilState != DepthStencilState.None )
				depthStencilState.Dispose();

			if( samplerState != Core.defaultSamplerState )
				samplerState.Dispose();
		}


		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// very basic here. We only check if the pointers are the same
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo( RenderState other )
		{
			if( object.ReferenceEquals( other, null ) )
				return 1;

			if( object.ReferenceEquals( this, other ) )
				return 0;

			return -1;
		}

	}
}

