using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// convenience subclass with a single property that casts the Effect for cleaner configuration
	/// </summary>
	public class Material<T> : Material, IDisposable where T : Effect
	{
		public T typedEffect { get { return (T)effect; } }


		public Material()
		{}


		public Material( T effect ) : base( effect )
		{}
	}


	public class Material : IComparable<Material>, IDisposable
	{
		/// <summary>
		/// default Material instance
		/// </summary>
		public static Material defaultMaterial = new Material();

		/// <summary>
		/// default opaque Material used for PostProcessors
		/// </summary>
		public static Material defaultOpaqueMaterial = new Material( BlendState.Opaque );

		/// <summary>
		/// BlendState used by the Batcher for the current RenderableComponent
		/// </summary>
		public BlendState blendState = BlendState.AlphaBlend;

		/// <summary>
		/// DepthStencilState used by the Batcher for the current RenderableComponent
		/// </summary>
		public DepthStencilState depthStencilState = DepthStencilState.None;

		/// <summary>
		/// SamplerState used by the Batcher for the current RenderableComponent
		/// </summary>
		public SamplerState samplerState = Core.defaultSamplerState;

		/// <summary>
		/// Effect used by the Batcher for the current RenderableComponent
		/// </summary>
		public Effect effect;


		#region Static common states

		// BlendStates can be made to work with transparency by adding the following:
		// - AlphaSourceBlend = Blend.SourceAlpha, 
		// - AlphaDestinationBlend = Blend.InverseSourceAlpha 

		public static Material stencilWrite( int stencilRef = 1 )
		{
			return new Material {
				depthStencilState = new DepthStencilState {
					StencilEnable = true,
					StencilFunction = CompareFunction.Always,
					StencilPass = StencilOperation.Replace,
					ReferenceStencil = stencilRef,
					DepthBufferEnable = false,
				}
			};
		}


		public static Material stencilRead( int stencilRef = 1 )
		{
			return new Material
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


		public static Material blendDarken()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Min
				}
			};
		}


		public static Material blendLighten()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Max
				}
			};
		}


		public static Material blendScreen()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static Material blendMultiply()
		{
			// works only for opaque textures
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.Zero,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		/// <summary>
		/// blend equation is sourceColor * sourceBlend + destinationColor * destinationBlend so this works out to sourceColor * destinationColor * 2
		/// and results in colors < 0.5 darkening and colors > 0.5 lightening the base
		/// </summary>
		public static Material blendMultiply2x()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.SourceColor,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static Material blendLinearDodge()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static Material blendLinearBurn()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.ReverseSubtract
				}
			};
		}


		public static Material blendDifference()
		{
			return new Material {
				blendState = new BlendState {
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.InverseSourceColor,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}


		public static Material blendSubtractive()
		{
			return new Material {
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


		public Material()
		{}


		public Material( Effect effect )
		{
			this.effect = effect;
		}


		public Material( BlendState blendState, Effect effect = null )
		{
			this.blendState = blendState;
			this.effect = effect;
		}


		public Material( DepthStencilState depthStencilState, Effect effect = null )
		{
			this.depthStencilState = depthStencilState;
			this.effect = effect;
		}


		~Material()
		{
			Dispose();
		}


		public virtual void Dispose()
		{
			if( blendState != null && blendState != BlendState.AlphaBlend )
			{
				blendState.Dispose();
				blendState = null;
			}

			if( depthStencilState != null && depthStencilState != DepthStencilState.None )
			{
				depthStencilState.Dispose();
				depthStencilState = null;
			}

			if( samplerState != null && samplerState != Core.defaultSamplerState )
			{
				samplerState.Dispose();
				samplerState = null;
			}

			if( effect != null )
			{
				effect.Dispose();
				effect = null;
			}
		}


		/// <summary>
		/// called when the Material is initialy set right before Batcher.begin to allow any Effects that have parameters set if necessary
		/// based on the Camera Matrix such as to set the MatrixTransform via camera.viewProjectionMatrix mimicking what Batcher does. This will
		/// only be called if there is a non-null Effect.
		/// </summary>
		/// <param name="camera">Camera.</param>
		public virtual void onPreRender( Camera camera )
		{}


		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// very basic here. We only check if the pointers are the same
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo( Material other )
		{
			if( object.ReferenceEquals( other, null ) )
				return 1;

			if( object.ReferenceEquals( this, other ) )
				return 0;

			return -1;
		}


		/// <summary>
		/// clones the Material. Note that the Effect is not cloned. It is the same instance as the original Material.
		/// </summary>
		public Material clone()
		{
			return new Material {
				blendState = blendState,
				depthStencilState = depthStencilState,
				samplerState = samplerState,
				effect = effect
			};
		}
	}
}

