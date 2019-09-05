using System;
using System.IO;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class EffectResource
	{
		// sprite effects
		internal static byte[] SpriteBlinkEffectBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/SpriteBlinkEffect.mgfxo"); }
		}

		internal static byte[] SpriteLinesEffectBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/SpriteLines.mgfxo"); }
		}

		internal static byte[] SpriteAlphaTestBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/SpriteAlphaTest.mgfxo"); }
		}

		internal static byte[] CrosshatchBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Crosshatch.mgfxo"); }
		}

		internal static byte[] NoiseBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Noise.mgfxo"); }
		}

		internal static byte[] TwistBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Twist.mgfxo"); }
		}

		internal static byte[] DotsBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Dots.mgfxo"); }
		}

		internal static byte[] DissolveBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Dissolve.mgfxo"); }
		}

		// post processor effects
		internal static byte[] BloomCombineBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/BloomCombine.mgfxo"); }
		}

		internal static byte[] BloomExtractBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/BloomExtract.mgfxo"); }
		}

		internal static byte[] GaussianBlurBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/GaussianBlur.mgfxo"); }
		}

		internal static byte[] VignetteBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Vignette.mgfxo"); }
		}

		internal static byte[] LetterboxBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Letterbox.mgfxo"); }
		}

		internal static byte[] HeatDistortionBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/HeatDistortion.mgfxo"); }
		}

		internal static byte[] SpriteLightMultiplyBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/SpriteLightMultiply.mgfxo"); }
		}

		internal static byte[] PixelGlitchBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/PixelGlitch.mgfxo"); }
		}

		// deferred lighting
		internal static byte[] DeferredSpriteBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/DeferredSprite.mgfxo"); }
		}

		internal static byte[] DeferredLightBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/DeferredLighting.mgfxo"); }
		}

		// forward lighting
		internal static byte[] ForwardLightingBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/ForwardLighting.mgfxo"); }
		}

		internal static byte[] PolygonLightBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/PolygonLight.mgfxo"); }
		}

		// scene transitions
		internal static byte[] SquaresTransitionBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/transitions/Squares.mgfxo"); }
		}

		// sprite or post processor effects
		internal static byte[] SpriteEffectBytes
		{
			get
			{
				return GetMonoGameEmbeddedResourceBytes(
					"Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo");
			}
		}

		internal static byte[] MultiTextureOverlayBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/MultiTextureOverlay.mgfxo"); }
		}

		internal static byte[] ScanlinesBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Scanlines.mgfxo"); }
		}

		internal static byte[] ReflectionBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Reflection.mgfxo"); }
		}

		internal static byte[] GrayscaleBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Grayscale.mgfxo"); }
		}

		internal static byte[] SepiaBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/Sepia.mgfxo"); }
		}

		internal static byte[] PaletteCyclerBytes
		{
			get { return GetFileResourceBytes("Content/nez/effects/PaletteCycler.mgfxo"); }
		}


		/// <summary>
		/// gets the raw byte[] from an EmbeddedResource
		/// </summary>
		/// <returns>The embedded resource bytes.</returns>
		/// <param name="name">Name.</param>
		static byte[] GetEmbeddedResourceBytes(string name)
		{
			var assembly = ReflectionUtils.GetAssembly(typeof(EffectResource));
			using (var stream = assembly.GetManifestResourceStream(name))
			{
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}


		internal static byte[] GetMonoGameEmbeddedResourceBytes(string name)
		{
#if FNA
			name = name.Replace( ".ogl.mgfxo", ".fxb" );
#endif

			var assembly = ReflectionUtils.GetAssembly(typeof(MathHelper));
			using (var stream = assembly.GetManifestResourceStream(name))
			{
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}


		/// <summary>
		/// fetches the raw byte data of a file from the Content folder. Used to keep the Effect subclass code simple and clean due to the Effect
		/// constructor requiring the byte[].
		/// </summary>
		/// <returns>The file resource bytes.</returns>
		/// <param name="path">Path.</param>
		public static byte[] GetFileResourceBytes(string path)
		{
#if FNA
			path = path.Replace( ".mgfxo", ".fxb" );
#endif

			byte[] bytes;
			try
			{
				using (var stream = TitleContainer.OpenStream(path))
				{
					if (stream.CanSeek)
					{
						bytes = new byte[stream.Length];
						stream.Read(bytes, 0, bytes.Length);
					}
					else
					{
						using (var ms = new MemoryStream())
						{
							stream.CopyTo(ms);
							bytes = ms.ToArray();
						}
					}
				}
			}
			catch (Exception e)
			{
				var txt = string.Format(
					"OpenStream failed to find file at path: {0}. Did you add it to the Content folder and set its properties to copy to output directory?",
					path);
				throw new Exception(txt, e);
			}

			return bytes;
		}
	}
}