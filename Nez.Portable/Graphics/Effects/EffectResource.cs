using System;
using System.IO;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class EffectResource
	{
		// sprite effects
		internal static byte[] spriteBlinkEffectBytes { get { return getFileResourceBytes( "Content/nez/effects/SpriteBlinkEffect.mgfxo" ); } }
		internal static byte[] spriteLinesEffectBytes { get { return getFileResourceBytes( "Content/nez/effects/SpriteLines.mgfxo" ); } }
		internal static byte[] spriteAlphaTestBytes { get { return getFileResourceBytes( "Content/nez/effects/SpriteAlphaTest.mgfxo" ); } }
		internal static byte[] crosshatchBytes { get { return getFileResourceBytes( "Content/nez/effects/Crosshatch.mgfxo" ); } }
		internal static byte[] noiseBytes { get { return getFileResourceBytes( "Content/nez/effects/Noise.mgfxo" ); } }
		internal static byte[] twistBytes { get { return getFileResourceBytes( "Content/nez/effects/Twist.mgfxo" ); } }
		internal static byte[] dotsBytes { get { return getFileResourceBytes( "Content/nez/effects/Dots.mgfxo" ); } }
		internal static byte[] dissolveBytes { get { return getFileResourceBytes( "Content/nez/effects/Dissolve.mgfxo" ); } }

		// post processor effects
		internal static byte[] bloomCombineBytes { get { return getFileResourceBytes( "Content/nez/effects/BloomCombine.mgfxo" ); } }
		internal static byte[] bloomExtractBytes { get { return getFileResourceBytes( "Content/nez/effects/BloomExtract.mgfxo" ); } }
		internal static byte[] gaussianBlurBytes { get { return getFileResourceBytes( "Content/nez/effects/GaussianBlur.mgfxo" ); } }
		internal static byte[] vignetteBytes { get { return getFileResourceBytes( "Content/nez/effects/Vignette.mgfxo" ); } }
		internal static byte[] letterboxBytes { get { return getFileResourceBytes( "Content/nez/effects/Letterbox.mgfxo" ); } }
		internal static byte[] heatDistortionBytes { get { return getFileResourceBytes( "Content/nez/effects/HeatDistortion.mgfxo" ); } }
		internal static byte[] spriteLightMultiplyBytes { get { return getFileResourceBytes( "Content/nez/effects/SpriteLightMultiply.mgfxo" ); } }
		internal static byte[] pixelGlitchBytes { get { return getFileResourceBytes( "Content/nez/effects/PixelGlitch.mgfxo" ); } }

		// deferred lighting
		internal static byte[] deferredSpriteBytes { get { return getFileResourceBytes( "Content/nez/effects/DeferredSprite.mgfxo" ); } }
		internal static byte[] deferredLightBytes { get { return getFileResourceBytes( "Content/nez/effects/DeferredLighting.mgfxo" ); } }

		// forward lighting
		internal static byte[] forwardLightingBytes { get { return getFileResourceBytes( "Content/nez/effects/ForwardLighting.mgfxo" ); } }
		internal static byte[] polygonLightBytes { get { return getFileResourceBytes( "Content/nez/effects/PolygonLight.mgfxo" ); } }

		// scene transitions
		internal static byte[] squaresTransitionBytes { get { return getFileResourceBytes( "Content/nez/effects/transitions/Squares.mgfxo" ); } }

		// sprite or post processor effects
		internal static byte[] spriteEffectBytes { get { return getMonoGameEmbeddedResourceBytes( "Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo" ); } }
		internal static byte[] multiTextureOverlayBytes { get { return getFileResourceBytes( "Content/nez/effects/MultiTextureOverlay.mgfxo" ); } }
		internal static byte[] scanlinesBytes { get { return getFileResourceBytes( "Content/nez/effects/Scanlines.mgfxo" ); } }
		internal static byte[] reflectionBytes { get { return getFileResourceBytes( "Content/nez/effects/Reflection.mgfxo" ); } }
		internal static byte[] grayscaleBytes { get { return getFileResourceBytes( "Content/nez/effects/Grayscale.mgfxo" ); } }
		internal static byte[] sepiaBytes { get { return getFileResourceBytes( "Content/nez/effects/Sepia.mgfxo" ); } }
		internal static byte[] paletteCyclerBytes { get { return getFileResourceBytes( "Content/nez/effects/PaletteCycler.mgfxo" ); } }


		/// <summary>
		/// gets the raw byte[] from an EmbeddedResource
		/// </summary>
		/// <returns>The embedded resource bytes.</returns>
		/// <param name="name">Name.</param>
		static byte[] getEmbeddedResourceBytes( string name )
		{
			var assembly = ReflectionUtils.getAssembly( typeof( EffectResource ) );
			using( var stream = assembly.GetManifestResourceStream( name ) )
			{
				using( var ms = new MemoryStream() )
				{
					stream.CopyTo( ms );
					return ms.ToArray();
				}
			}
		}


		internal static byte[] getMonoGameEmbeddedResourceBytes( string name )
		{
			#if FNA
			name = name.Replace( ".ogl.mgfxo", ".fxb" );
			#endif

			var assembly = ReflectionUtils.getAssembly( typeof( MathHelper ) );
			using( var stream = assembly.GetManifestResourceStream( name ) )
			{
				using( var ms = new MemoryStream() )
				{
					stream.CopyTo( ms );
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
		public static byte[] getFileResourceBytes( string path )
		{
			#if FNA
			path = path.Replace( ".mgfxo", ".fxb" );
			#endif

			byte[] bytes;
			try
			{
				using( var stream = TitleContainer.OpenStream( path ) )
				{
					bytes = new byte[stream.Length];
					stream.Read( bytes, 0, bytes.Length );
				}
			}
			catch( Exception e )
			{
				var txt = string.Format( "OpenStream failed to find file at path: {0}. Did you add it to the Content folder and set its properties to copy to output directory?", path );
				throw new Exception( txt, e );
			}

			return bytes;
		}
	}
}

