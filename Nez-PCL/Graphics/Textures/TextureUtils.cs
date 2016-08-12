using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public static class TextureUtils
	{
		public enum EdgeDetectionFilter
		{
			Sobel,
			Scharr,
			FiveTap
		}

		
		/// <summary>
		/// processes each pixel of the passed in Texture and in the output texture transparent pixels will be transparentColor and opaque pixels
		/// will be opaqueColor. This is useful for creating normal maps for rim lighting by applying a grayscale blur then using createNormalMap*
		/// by doing something like the following. The first step is used only for making rim lighting normal maps:
		/// - var maskTex = createFlatHeightmap( tex, Color.White, Color.Black )
		/// - var blurredTex = createBlurredGrayscaleTexture( maskTex, 1 )
		/// - createNormalMap( blurredTex, 50f )
		/// </summary>
		/// <returns>The flat heightmap.</returns>
		/// <param name="image">Image.</param>
		/// <param name="opaqueColor">Opaque color.</param>
		/// <param name="transparentColor">Transparent color.</param>
		public static Texture2D createFlatHeightmap( Texture2D image, Color opaqueColor, Color transparentColor )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );

			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );

			var destData = createFlatHeightmap( srcData, opaqueColor, transparentColor );

			resultTex.SetData( destData );

			return resultTex;
		}


		public static Color[] createFlatHeightmap( Color[] srcData, Color opaqueColor, Color transparentColor )
		{
			var destData = new Color[srcData.Length];

			for( var i = 0; i < srcData.Length; i++ )
			{
				var pixel = srcData[i];

				if( pixel.A == 0 )
					destData[i] = transparentColor;
				else
					destData[i] = opaqueColor;
			}

			return destData;
		}

		
		/// <summary>
		/// creates a new texture that is a gaussian blurred version of the original in grayscale
		/// </summary>
		/// <returns>The blurred texture.</returns>
		/// <param name="image">Image.</param>
		/// <param name="deviation">Deviation.</param>
		public static Texture2D createBlurredGrayscaleTexture( Texture2D image, double deviation = 1 )
		{
			return GaussianBlur.createBlurredGrayscaleTexture( image, deviation );
		}


		public static Color[] createBlurredTexture( Color[] srcData, int width, int height, double deviation = 1 )
		{
			return GaussianBlur.createBlurredTexture( srcData, width, height, deviation );
		}


		/// <summary>
		/// creates a new texture that is a gaussian blurred version of the original
		/// </summary>
		/// <returns>The blurred texture.</returns>
		/// <param name="image">Image.</param>
		/// <param name="deviation">Deviation.</param>
		public static Texture2D createBlurredTexture( Texture2D image, double deviation = 1 )
		{
			return GaussianBlur.createBlurredTexture( image, deviation );
		}


		public static Color[] createBlurredGrayscaleTexture( Color[] srcData, int width, int height, double deviation = 1 )
		{
			return GaussianBlur.createBlurredGrayscaleTexture( srcData, width, height, deviation );
		}


		/// <summary>
		/// generates a normal map from a height map calculating it with a sobel filter
		/// </summary>
		/// <returns>The sobel filter.</returns>
		/// <param name="image">Image.</param>
		/// <param name="normalStrength">Normal strength.</param>
		public static Texture2D createNormalMap( Texture2D image, EdgeDetectionFilter filter, float normalStrength = 1f, bool invertX = false, bool invertY = false )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );

			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );

			var destData = createNormalMap( srcData, filter, image.Width, image.Height, normalStrength, invertX, invertY );
			resultTex.SetData( destData );

			return resultTex;
		}


		public static Color[] createNormalMap( Color[] srcData, EdgeDetectionFilter filter, int width, int height, float normalStrength = 1f, bool invertX = false, bool invertY = false )
		{
			// TODO: why does teh scharr algorithm require us to flip the y axis?
			if( filter == EdgeDetectionFilter.Scharr )
				invertY = !invertY;
			
			var invertR = invertX ? -1f : 1f;
			var invertG = invertY ? -1f : 1f;
			var destData = new Color[width * height];

			for( var i = 1; i < width - 1; i++ )
			{
				for( var j = 1; j < height - 1; j++ )
				{
					var c = srcData[i + j * width].grayscale().B / 255f;
					var r = srcData[i + 1 + j * width].grayscale().B / 255f;
					var l = srcData[i - 1 + j * width].grayscale().B / 255f;
					var t = srcData[i + ( j - 1 ) * width].grayscale().B / 255f;
					var b = srcData[i + ( j + 1 ) * width].grayscale().B / 255f;
					var bl = srcData[i - 1 + ( j + 1 ) * width].grayscale().B / 255f;
					var tl = srcData[i - 1 + ( j - 1 ) * width].grayscale().B / 255f;
					var br = srcData[i + 1 + ( j + 1 ) * width].grayscale().B / 255f;
					var tr = srcData[i + 1 + ( j - 1 ) * width].grayscale().B / 255f;

					float dX = 0f, dY = 0f;
					switch( filter )
					{
						case EdgeDetectionFilter.Sobel:
							dX = tl + l * 2 + bl - tr - r * 2 - br;
							dY = bl + 2 * b + br - tl - 2 * t - tr;
						break;
						case EdgeDetectionFilter.Scharr:
							dX = tl * 3 + l * 10 + bl * 3 - tr * 3 - r * 10 - br * 3;
							dY = tl * 3 + t * 10 + tr * 3 - bl * 3 - b * 10 - br * 3;
						break;
						case EdgeDetectionFilter.FiveTap:
							dX = ( ( l - c ) + ( c - r ) ) * 0.5f;
							dY = ( ( b - c ) + ( c - t ) ) * 0.5f;
						break;
					}

					var normal = Vector3.Normalize( new Vector3( dX * invertR, dY * invertG, 1 / normalStrength ) );
					normal = normal * 0.5f + new Vector3( 0.5f );
					destData[i + j * width] = new Color( normal.X, normal.Y, normal.Z, srcData[i + j * width].A );
				}
			}

			return destData;
		}

	}
}

