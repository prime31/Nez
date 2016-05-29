using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Textures
{
	public class GaussianBlur
	{
		/// <summary>
		/// creates a new texture that is a gaussian blurred version of the original
		/// </summary>
		/// <returns>The blurred texture.</returns>
		/// <param name="image">Image.</param>
		/// <param name="deviation">Deviation.</param>
		public static Texture2D createdBlurredTexture( Texture2D image, double deviation = 1 )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );
			var matrix = new double[image.Width, image.Height];

			var destData = new Color[image.Width * image.Height];
			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );


			for( var i = 0; i < image.Width; i++ )
			{
				for( var j = 0; j < image.Height; j++ )
					matrix[i, j] = srcData[i + j * image.Width].grayscale().R;
			}

			matrix = GaussianBlur.gaussianConvolution( matrix, deviation );
			for( var i = 0; i < image.Width; i++ )
			{
				for( var j = 0; j < image.Height; j++ )
				{
					var val = (int)Math.Min( 255, matrix[i, j] );
					destData[i + j * image.Width] = new Color( val, val, val, 255 );
				}
			}

			resultTex.SetData( destData );

			return resultTex;
		}


		static double[,] calculate1DSampleKernel( double deviation, int size )
		{
			double[,] ret = new double[size, 1];
			double sum = 0;
			//int half = size / 2; // originally used this but I dont think it is correct since it creates incorrect half values
			double half = ( size - 1 ) / 2;
			for( var i = 0; i < size; i++ )
			{
				ret[i, 0] = 1 / ( Math.Sqrt( 2 * Math.PI ) * deviation ) * Math.Exp( -( i - half ) * ( i - half ) / ( 2 * deviation * deviation ) );
				sum += ret[i, 0];
			}
			return ret;
		}


		static double[,] calculate1DSampleKernel( double deviation )
		{
			var size = (int)Math.Ceiling( deviation * 3 ) * 2 + 1;
			return calculate1DSampleKernel( deviation, size );
		}


		static double[,] calculateNormalized1DSampleKernel( double deviation )
		{
			return normalizeMatrix( calculate1DSampleKernel( deviation ) );
		}


		static double[,] normalizeMatrix( double[,] matrix )
		{
			var ret = new double[matrix.GetLength( 0 ), matrix.GetLength( 1 )];
			double sum = 0;
			for( var i = 0; i < ret.GetLength( 0 ); i++ )
			{
				for( var j = 0; j < ret.GetLength( 1 ); j++ )
					sum += matrix[i, j];
			}

			if( sum != 0 )
			{
				for( var i = 0; i < ret.GetLength( 0 ); i++ )
				{
					for( var j = 0; j < ret.GetLength( 1 ); j++ )
						ret[i, j] = matrix[i, j] / sum;
				}
			}
			return ret;
		}


		static double[,] gaussianConvolution( double[,] matrix, double deviation )
		{
			var kernel = calculateNormalized1DSampleKernel( deviation );
			var res1 = new double[matrix.GetLength( 0 ), matrix.GetLength( 1 )];
			var res2 = new double[matrix.GetLength( 0 ), matrix.GetLength( 1 )];

			// x-direction
			for( var i = 0; i < matrix.GetLength( 0 ); i++ )
			{
				for( var j = 0; j < matrix.GetLength( 1 ); j++ )
					res1[i, j] = processPoint( matrix, i, j, kernel, 0 );
			}

			// y-direction
			for( var i = 0; i < matrix.GetLength( 0 ); i++ )
			{
				for( var j = 0; j < matrix.GetLength( 1 ); j++ )
					res2[i, j] = processPoint( res1, i, j, kernel, 1 );
			}
			return res2;
		}


		static double processPoint( double[,] matrix, int x, int y, double[,] kernel, int direction )
		{
			double res = 0;
			var half = kernel.GetLength( 0 ) / 2;
			for( var i = 0; i < kernel.GetLength( 0 ); i++ )
			{
				var cox = direction == 0 ? x + i - half : x;
				var coy = direction == 1 ? y + i - half : y;
				if( cox >= 0 && cox < matrix.GetLength( 0 ) && coy >= 0 && coy < matrix.GetLength( 1 ) )
					res += matrix[cox, coy] * kernel[i, 0];
			}
			return res;
		}

	}
}

