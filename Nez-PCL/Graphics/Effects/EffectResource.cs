using System;
using System.IO;
using Microsoft.Xna.Framework;

#if WINRT
using System.Reflection;
#endif


namespace Nez
{
	public static class EffectResource
	{
		// sprite effects
		internal static byte[] spriteBlinkEffectBytes { get { return getFileResourceBytes( "Content/nez/effects/SpriteBlinkEffect.mgfxo" ); } }
		internal static byte[] crosshatchBytes { get { return getFileResourceBytes( "Content/nez/effects/Crosshatch.mgfxo" ); } }
		internal static byte[] noiseBytes { get { return getFileResourceBytes( "Content/nez/effects/Noise.mgfxo" ); } }
		internal static byte[] twistBytes { get { return getFileResourceBytes( "Content/nez/effects/Twist.mgfxo" ); } }
		internal static byte[] dotsBytes { get { return getFileResourceBytes( "Content/nez/effects/Dots.mgfxo" ); } }

		// post processor effects
		internal static byte[] bloomCombineBytes { get { return getFileResourceBytes( "Content/nez/effects/BloomCombine.mgfxo" ); } }
		internal static byte[] bloomExtractBytes { get { return getFileResourceBytes( "Content/nez/effects/BloomExtract.mgfxo" ); } }
		internal static byte[] gaussianBlurBytes { get { return getFileResourceBytes( "Content/nez/effects/GaussianBlur.mgfxo" ); } }
		internal static byte[] vignetteBytes { get { return getFileResourceBytes( "Content/nez/effects/Vignette.mgfxo" ); } }
		internal static byte[] heatDistortionBytes { get { return getFileResourceBytes( "Content/nez/effects/HeatDistortion.mgfxo" ); } }

		// scene transitions
		internal static byte[] squaresTransitionBytes { get { return getFileResourceBytes( "Content/nez/effects/transitions/Squares.mgfxo" ); } }

		internal static byte[] multiTextureOverlayBytes { get { return getFileResourceBytes( "Content/nez/effects/MultiTextureOverlay.mgfxo" ); } }
		internal static byte[] scanlinesBytes { get { return getFileResourceBytes( "Content/nez/effects/Scanlines.mgfxo" ); } }


		/// <summary>
		/// gets the raw byte[] from an EmbeddedResource
		/// </summary>
		/// <returns>The embedded resource bytes.</returns>
		/// <param name="name">Name.</param>
		static byte[] getEmbeddedResourceBytes( string name )
		{
			#if WINRT
			var assembly = typeof(EffectResource).GetTypeInfo().Assembly;
			#else
			var assembly = typeof( EffectResource ).Assembly;
			#endif

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
			byte[] bytes;
			using( var stream = TitleContainer.OpenStream( path ) )
			{
				bytes = new byte[stream.Length];
				stream.Read( bytes, 0, bytes.Length );
			}

			return bytes;
		}
	}
}

