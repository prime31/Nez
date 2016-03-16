using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class ColorExt
	{
		private const string HEX = "0123456789ABCDEF";


		static public byte hexToByte( char c )
		{
			return (byte)HEX.IndexOf( char.ToUpper( c ) );
		}


		static public Color invert( this Color color )
		{
			return new Color( 255 - color.R, 255 - color.G, 255 - color.B, color.A );
		}


		static public Color hexToColor( string hex )
		{
			float r = ( hexToByte( hex[0] ) * 16 + hexToByte( hex[1] ) ) / 255.0f;
			float g = ( hexToByte( hex[2] ) * 16 + hexToByte( hex[3] ) ) / 255.0f;
			float b = ( hexToByte( hex[4] ) * 16 + hexToByte( hex[5] ) ) / 255.0f;

			return new Color( r, g, b );
		}


		public static Color add( ref Color first, ref Color second )
		{
			return new Color( first.R + second.R, first.G + second.G, first.B + second.B, first.A + second.A );
		}


		public static void add( ref Color first, ref Color second, out Color result )
		{
			result = new Color();
			result.R = (byte)( first.R + second.R );
			result.G = (byte)( first.G + second.G );
			result.B = (byte)( first.B + second.B );
			result.A = (byte)( first.A + second.A );
		}


		/// <summary>
		/// first - second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		public static Color subtract( ref Color first, ref Color second )
		{
			return new Color( first.R - second.R, first.G - second.G, first.B - second.B, first.A - second.A );
		}


		/// <summary>
		/// first - second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="result">Result.</param>
		public static void subtract( ref Color first, ref Color second, out Color result )
		{
			result = new Color();
			result.R = (byte)( first.R - second.R );
			result.G = (byte)( first.G - second.G );
			result.B = (byte)( first.B - second.B );
			result.A = (byte)( first.A - second.A );
		}


		public static Color multiply( this Color self, Color second )
		{
			return new Color {
				R = (byte)( self.R * second.R ),
				G = (byte)( self.G * second.G ),
				B = (byte)( self.B * second.B ),
				A = (byte)( self.A * second.A )
			};
		}


		/// <summary>
		/// linearly interpolates Color from - to
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="t">T.</param>
		public static Color lerp( ref Color from, ref Color to, float t )
		{
			var t256 = (int)( t * 256 );
			return new Color( from.R + ( to.R - from.R ) * t256 / 256, from.G + ( to.G - from.G ) * t256 / 256, from.B + ( to.B - from.B ) * t256 / 256, from.A + ( to.A - from.A ) * t256 / 256 );
		}


		/// <summary>
		/// linearly interpolates Color from - to
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="t">T.</param>
		public static void lerp( ref Color from, ref Color to, out Color result, float t )
		{
			result = new Color();
			var t256 = (int)( t * 256 );
			result.R = (byte)( from.R + ( to.R - from.R ) * t256 / 256 );
			result.G = (byte)( from.G + ( to.G - from.G ) * t256 / 256 );
			result.B = (byte)( from.B + ( to.B - from.B ) * t256 / 256 );
			result.A = (byte)( from.A + ( to.A - from.A ) * t256 / 256 );
		}

	}
}

