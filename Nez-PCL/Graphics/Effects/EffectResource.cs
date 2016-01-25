using System;
using System.IO;

#if WINRT
using System.Reflection;
#endif


namespace Nez
{
	static internal class EffectResource
	{
		public static byte[] crosshatchBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.Crosshatch.mgfxo" ); } }
		public static byte[] noiseBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.Noise.mgfxo" ); } }
		public static byte[] twistBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.Twist.mgfxo" ); } }
		public static byte[] dotsBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.Dots.mgfxo" ); } }

		public static byte[] bloomCombineBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.BloomCombine.mgfxo" ); } }
		public static byte[] bloomExtractBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.BloomExtract.mgfxo" ); } }
		public static byte[] gaussianBlurBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.GaussianBlur.mgfxo" ); } }
		public static byte[] multiTextureOverlayBytes { get { return getEmbeddedResourceBytes( "Nez.Content.Effects.MultiTextureOverlay.mgfxo" ); } }


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
	}
}

