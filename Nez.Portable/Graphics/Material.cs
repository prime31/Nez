using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// convenience subclass with a single property that casts the Effect for cleaner configuration
	/// </summary>
	public class Material<T> : Material, IDisposable where T : Effect
	{
		public new T Effect
		{
			get => (T) base.Effect;
			set => base.Effect = value;
		}

		public Material()
		{
		}

		public Material(T effect) : base(effect)
		{
		}
	}


	public class Material : IComparable<Material>, IDisposable
	{
		/// <summary>
		/// default Material instance
		/// </summary>
		public static Material DefaultMaterial = new Material();

		/// <summary>
		/// default opaque Material used for PostProcessors
		/// </summary>
		public static Material DefaultOpaqueMaterial = new Material(BlendState.Opaque);

		/// <summary>
		/// BlendState used by the Batcher for the current RenderableComponent
		/// </summary>
		public BlendState BlendState = BlendState.AlphaBlend;

		/// <summary>
		/// DepthStencilState used by the Batcher for the current RenderableComponent
		/// </summary>
		public DepthStencilState DepthStencilState = DepthStencilState.None;

		/// <summary>
		/// SamplerState used by the Batcher for the current RenderableComponent
		/// </summary>
		public SamplerState SamplerState = Core.DefaultSamplerState;

		/// <summary>
		/// Effect used by the Batcher for the current RenderableComponent
		/// </summary>
		public Effect Effect;


		#region Static common states

		// BlendStates can be made to work with transparency by adding the following:
		// - AlphaSourceBlend = Blend.SourceAlpha, 
		// - AlphaDestinationBlend = Blend.InverseSourceAlpha 

		public static Material StencilWrite(int stencilRef = 1)
		{
			return new Material
			{
				DepthStencilState = new DepthStencilState
				{
					StencilEnable = true,
					StencilFunction = CompareFunction.Always,
					StencilPass = StencilOperation.Replace,
					ReferenceStencil = stencilRef,
					DepthBufferEnable = false,
				}
			};
		}

		public static Material StencilRead(int stencilRef = 1)
		{
			return new Material
			{
				DepthStencilState = new DepthStencilState
				{
					StencilEnable = true,
					StencilFunction = CompareFunction.Equal,
					StencilPass = StencilOperation.Keep,
					ReferenceStencil = stencilRef,
					DepthBufferEnable = false
				}
			};
		}

		public static Material BlendDarken()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Min,
					AlphaSourceBlend = Blend.One,
					AlphaDestinationBlend = Blend.One,
					AlphaBlendFunction = BlendFunction.Min
				}
			};
		}

		public static Material BlendLighten()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Max,
					AlphaSourceBlend = Blend.One,
					AlphaDestinationBlend = Blend.One,
					AlphaBlendFunction = BlendFunction.Max
				}
			};
		}

		public static Material BlendScreen()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}

		public static Material BlendMultiply()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.Zero,
					ColorBlendFunction = BlendFunction.Add,
					AlphaSourceBlend = Blend.DestinationAlpha,
					AlphaDestinationBlend = Blend.Zero,
					AlphaBlendFunction = BlendFunction.Add
				}
			};
		}


		/// <summary>
		/// blend equation is sourceColor * sourceBlend + destinationColor * destinationBlend so this works out to sourceColor * destinationColor * 2
		/// and results in colors < 0.5 darkening and colors > 0.5 lightening the base
		/// </summary>
		public static Material BlendMultiply2x()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.DestinationColor,
					ColorDestinationBlend = Blend.SourceColor,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}

		public static Material BlendLinearDodge()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}

		public static Material BlendLinearBurn()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.One,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.ReverseSubtract
				}
			};
		}

		public static Material BlendDifference()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.InverseDestinationColor,
					ColorDestinationBlend = Blend.InverseSourceColor,
					ColorBlendFunction = BlendFunction.Add
				}
			};
		}

		public static Material BlendSubtractive()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.SourceAlpha,
					ColorDestinationBlend = Blend.One,
					ColorBlendFunction = BlendFunction.ReverseSubtract,
					AlphaSourceBlend = Blend.SourceAlpha,
					AlphaDestinationBlend = Blend.One,
					AlphaBlendFunction = BlendFunction.ReverseSubtract
				}
			};
		}

		public static Material BlendAdditive()
		{
			return new Material
			{
				BlendState = new BlendState
				{
					ColorSourceBlend = Blend.SourceAlpha,
					ColorDestinationBlend = Blend.One,
					AlphaSourceBlend = Blend.SourceAlpha,
					AlphaDestinationBlend = Blend.One
				}
			};
		}

		#endregion


		public Material()
		{
		}

		public Material(Effect effect)
		{
			Effect = effect;
		}

		public Material(BlendState blendState, Effect effect = null)
		{
			BlendState = blendState;
			Effect = effect;
		}

		public Material(DepthStencilState depthStencilState, Effect effect = null)
		{
			DepthStencilState = depthStencilState;
			Effect = effect;
		}

		~Material()
		{
			Dispose();
		}

		public virtual void Dispose()
		{
			// dispose of our state only if they are not using the shared instances
			if (BlendState != null && BlendState != BlendState.AlphaBlend)
			{
				BlendState.Dispose();
				BlendState = null;
			}

			if (DepthStencilState != null && DepthStencilState != DepthStencilState.None)
			{
				DepthStencilState.Dispose();
				DepthStencilState = null;
			}

			if (SamplerState != null && SamplerState != Core.DefaultSamplerState)
			{
				SamplerState.Dispose();
				SamplerState = null;
			}

			if (Effect != null)
			{
				Effect.Dispose();
				Effect = null;
			}
		}

		/// <summary>
		/// called when the Material is initialy set right before Batcher.begin to allow any Effects that have parameters set if necessary
		/// based on the Camera Matrix such as to set the MatrixTransform via camera.viewProjectionMatrix mimicking what Batcher does. This will
		/// only be called if there is a non-null Effect.
		/// </summary>
		/// <param name="camera">Camera.</param>
		public virtual void OnPreRender(Camera camera)
		{
		}

		/// <summary>
		/// very basic here. We only check if the pointers are the same
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo(Material other)
		{
			if (ReferenceEquals(other, null))
				return 1;

			if (ReferenceEquals(this, other))
				return 0;

			return -1;
		}

		/// <summary>
		/// clones the Material. Note that the Effect is not cloned. It is the same instance as the original Material.
		/// </summary>
		public Material Clone()
		{
			return new Material
			{
				BlendState = BlendState,
				DepthStencilState = DepthStencilState,
				SamplerState = SamplerState,
				Effect = Effect
			};
		}
	}
}